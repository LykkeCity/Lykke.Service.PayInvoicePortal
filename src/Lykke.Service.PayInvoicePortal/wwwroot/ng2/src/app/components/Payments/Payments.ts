import { Component, OnInit } from '@angular/core';
import { PagerModel } from '../../models/PagerModel';
import { ConfirmModalService } from '../../services/ConfirmModalService';
import { PaymentsFilterModel } from './PaymentsFilter/PaymentsFilterModel';

@Component({
  selector: PaymentsComponent.Selector,
  templateUrl: './Payments.html'
})
export class PaymentsComponent implements OnInit {
  static readonly Selector = 'lp-payments';

  model = new PaymentsModel();
  view = new View();
  hanlders = {
    exportToCsv: this.exportToCsv,
    remove: this.remove,
    showMore: this.showMore
  };
  pager = new PagerModel(20);

  constructor(private confirmModalService: ConfirmModalService) {
    this.view.handlers = {
      getStatusCss: this.getStatusCss,
      canRemove: this.canRemove
    };
  }

  ngOnInit() {}

  private exportToCsv() {
    console.log('exportToCsv');
  }

  private remove(payment: PaymentModel): void {
    if (!this.canRemove(payment)) {
      return;
    }

    this.confirmModalService.openModal({
      content: `Are you sure you want to remove this invoice #${
        payment.number
      }?`,
      yesAction: () => {
        this.view.isLoadingDeletePayment = true;

        // TODO:
        // this.api.deletePayment(payment.id).subscribe(
        //   res => {
        //     //load
        //   },
        //   error => {
        //     console.error(error);
        //     this.confirmModalService.showErrorModal();
        //     this.view.isLoadingDeletePayment = false;
        //   }
        // );
      }
    });
  }

  private showMore(): void {
    console.log('showMore'); // TODO:
  }

  private getStatusCss(status: string): string {
    console.log(status); // TODO:
    return null;
  }

  private canRemove(payment: PaymentModel): boolean {
    return (
      payment &&
      payment.type === PaymentType.Invoice &&
      (payment.status === 'Draft' || payment.status === 'Unpaid')
    );
  }

  private onFilterChanged(filter: PaymentsFilterModel) {
    console.log(filter); // TODO:
  }
}

class PaymentsModel {
  baseAsset: null;
  payments: PaymentModel[];
}

class PaymentModel {
  id: string;
  get type(): PaymentType {
    return this.number ? PaymentType.Invoice : PaymentType.Api;
  }
  number: string;
  createdDate: Date;
  clientName: string;
  clientEmail: string;
  amount: number;
  settlementAsset: string;
  settlementAssetAccuracy: number;
  status: string;
  dueDate: Date;
}

enum PaymentType {
  Invoice,
  Api
}

class View {
  isFirstLoading: boolean;
  isLoading: boolean;
  isLoadingDeletePayment: boolean;
  handlers: {};
  constructor() {
    this.isFirstLoading = true;
  }
}
