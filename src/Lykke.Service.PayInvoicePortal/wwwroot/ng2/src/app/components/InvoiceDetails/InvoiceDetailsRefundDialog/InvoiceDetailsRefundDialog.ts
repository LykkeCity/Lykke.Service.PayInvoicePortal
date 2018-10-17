import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RefundRequest } from './RefundRequest';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorResponse } from 'src/app/models/ErrorResponse';
import { RefundDialogModel } from './RefundDialogModel';
import { RefundApi } from 'src/app/services/api/RefundApi';
import { RefundDataResponse } from './RefundDataResponse';
import { AssetItemViewModel } from 'src/app/models/AssetItemViewModels';

@Component({
  selector: InvoiceDetailsRefundDialogComponent.Selector,
  templateUrl: './InvoiceDetailsRefundDialog.html'
})
export class InvoiceDetailsRefundDialogComponent
  implements IInvoiceDetailsRefundDialogHandlers {
  static readonly Selector = 'lp-invoice-details-refund-dialog';

  @Input()
  show: boolean;

  @Input()
  paymentRequestId: string;

  @Output()
  closeDialog = new EventEmitter();

  @Output()
  refreshInvoice = new EventEmitter();

  model = new RefundDialogModel();
  view = new View();
  loadingValidation = new LoadingValidation();
  validation = new Validation();

  constructor(private api: RefundApi) {}

  loadRefundData(): void {
    this.view.isFirstLoading = true;
    this.loadingValidation.reset();

    this.api.getRefundData(this.paymentRequestId).subscribe(
      (res: RefundDataResponse) => {
        this.view.isFirstLoading = false;

        if (!RefundDataResponse.isValid(res)) {
          this.loadingValidation.unexpectedError = true;
          return;
        }

        this.model.paymentAsset = res.paymentAsset;
        this.model.selectedPaymentAssetId = res.paymentAsset.id;
        this.model.paymentAssets = [
          new AssetItemViewModel(
            res.paymentAsset.id,
            res.paymentAsset.displayId
          )
        ];
        this.model.selectedWalletAddress = res.sourceWalletAddresses[0];
        this.model.sourceWalletAddresses = res.sourceWalletAddresses.map(
          value => new AssetItemViewModel(value, value)
        );
        this.model.amount = res.amount;
      },
      (httpResponseError: HttpErrorResponse) => {
        console.error(httpResponseError);
        this.view.isFirstLoading = false;

        const errorResponse = httpResponseError.error as ErrorResponse;

        if (errorResponse) {
          if (errorResponse.errorMessage) {
            switch (errorResponse.errorMessage) {
              case 'NotAllowedInStatus':
                this.loadingValidation.notAllowedInStatus = true;
                return;
            }
          }
        }

        this.loadingValidation.unexpectedError = true;
      }
    );
  }

  refund(): void {
    this.view.isLoading = true;
    this.validation.reset();

    const model = new RefundRequest(
      this.paymentRequestId,
      this.model.manualAddress && this.model.manualAddress.length
        ? this.model.manualAddress
        : this.model.selectedWalletAddress
    );

    this.api.refund(model).subscribe(
      res => {
        this.view.isLoading = false;
        this.view.isSuccess = true;
        this.refreshInvoice.emit();
      },
      (httpResponseError: HttpErrorResponse) => {
        console.error(httpResponseError);
        this.view.isLoading = false;

        const errorResponse = httpResponseError.error as ErrorResponse;

        if (errorResponse) {
          if (errorResponse.errorMessage) {
            switch (errorResponse.errorMessage) {
              case 'InvalidDestinationAddress':
                this.validation.invalidDestinationAddress = true;
                return;
              case 'NotAllowedInStatus':
                this.loadingValidation.notAllowedInStatus = true;
                return;
            }
          }
        }

        this.validation.unexpectedError = true;
      }
    );
  }

  close(): void {
    this.view.isFirstLoading = true;
    this.view.isSuccess = false;
    this.closeDialog.emit();
  }

  overlayClick(e: MouseEvent): void {
    if (this.show) {
      e.preventDefault();
      e.stopPropagation();

      this.close();
    }
  }
}

interface IInvoiceDetailsRefundDialogHandlers {
  loadRefundData: () => void;
}

class View {
  isFirstLoading: boolean;
  isLoading: boolean;
  isSuccess: boolean;

  constructor() {
    this.isFirstLoading = true;
  }
}

class LoadingValidation {
  notAllowedInStatus: boolean;
  unexpectedError: boolean;
  reset(): void {
    this.notAllowedInStatus = false;
    this.unexpectedError = false;
  }

  get hasError(): boolean {
    return this.notAllowedInStatus || this.unexpectedError;
  }
}

class Validation {
  invalidDestinationAddress: boolean;
  unexpectedError: boolean;
  reset(): void {
    this.invalidDestinationAddress = false;
    this.unexpectedError = false;
  }
}
