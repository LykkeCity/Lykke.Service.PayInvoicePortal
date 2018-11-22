import { Input, Component, Output, EventEmitter } from '@angular/core';
import { PaymentModel } from '../../../models/Payment/PaymentModel';
import { PaymentStatus } from '../../../models/Payment/PaymentStatus';
import { PaymentType } from '../../../models/Payment/PaymentType';
import { ConfirmModalService } from '../../../services/ConfirmModalService';
import { PaymentsApi } from '../../../services/api/PaymentsApi';
import { PaymentStatusCssService } from '../../../services/Payment/PaymentStatusCssService';

@Component({
  selector: PaymentsTableComponent.Selector,
  templateUrl: './PaymentsTable.html'
})
export class PaymentsTableComponent implements IPaymentsTableHandlers {
  static readonly Selector = 'lp-payments-table';

  @Input()
  payments: PaymentModel[];

  @Input()
  hasMorePayments: boolean;

  @Input()
  showMoreLoading: boolean;

  @Output()
  showMoreEvent = new EventEmitter();

  @Output()
  paymentRemovedEvent = new EventEmitter();

  @Output()
  paymentUpdatedEvent = new EventEmitter();

  view = new View();

  constructor(
    private api: PaymentsApi,
    private confirmModalService: ConfirmModalService,
    private paymentStatusCssService: PaymentStatusCssService
  ) {
    this.view.canRemove = this.canRemove;
    this.view.getStatusCss = (status) => this.getStatusCss(status);
  }

  showMore(): void {
    this.showMoreEvent.emit();
  }

  remove(payment: PaymentModel, index: number): void {
    if (!this.canRemove(payment)) {
      return;
    }

    this.confirmModalService.openModal({
      content: `Are you sure you want to remove this invoice #${
        payment.number
      }?`,
      yesAction: () => {
        payment.isLoadingDeletePayment = true;

        this.api.deleteInvoice(payment.id).subscribe(
          res => {
            this.confirmModalService.openModal({
              title: 'Success',
              content: `Invoice #${payment.number} successfully removed.`
            });

            if (payment.status === PaymentStatus[PaymentStatus.Unpaid]) {
              this.paymentUpdatedEvent.emit({
                id: payment.id,
                status: payment.status
              });
              payment.isLoadingDeletePayment = false;
            } else {
              this.paymentRemovedEvent.emit(payment.id);
            }
          },
          error => {
            console.error(error);
            this.confirmModalService.showErrorModal();
            payment.isLoadingDeletePayment = false;
          }
        );
      }
    });
  }

  private canRemove(payment: PaymentModel): boolean {
    const result =
      payment &&
      payment.type === PaymentType.Invoice &&
      (payment.status === PaymentStatus[PaymentStatus.Draft] ||
        payment.status === PaymentStatus[PaymentStatus.Unpaid]);
    return result;
  }

  private getStatusCss(status: string): string {
    return this.paymentStatusCssService.getStatusCss(PaymentStatus[status]);
  }
}

interface IPaymentsTableHandlers {
  showMore: () => void;
  remove: (payment: PaymentModel, index: number) => void;
}

class View {
  isLoadingDeletePayment: boolean;
  canRemove: (payment: PaymentModel) => boolean;
  getStatusCss: (status: string) => string;
}
