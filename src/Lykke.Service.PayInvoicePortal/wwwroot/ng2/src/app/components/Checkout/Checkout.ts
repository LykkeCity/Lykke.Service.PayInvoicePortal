import { OnInit, Component, OnDestroy } from '@angular/core';
import { CheckoutModel } from './CheckoutModel';
import { FileModel } from 'src/app/models/FileModel';
import { FileService } from 'src/app/services/FileService';
import { ConfirmModalService } from 'src/app/services/ConfirmModalService';
import { getGuidFromPath, getParameterByName } from 'src/app/utils/utils';
import {
  ROUTE_INVOICE_CHECKOUT_PAGE,
  ROUTE_ERROR_PAGE
} from 'src/app/constants/routes';
import { CheckoutResponse } from './CheckoutResponseModel';
import { PaymentStatusCssService } from 'src/app/services/Payment/PaymentStatusCssService';
import { PaymentStatus } from 'src/app/models/Payment/PaymentStatus';
import { CheckoutApi } from 'src/app/services/api/CheckoutApi';
import { AssetsApi } from 'src/app/services/api/AssetsApi';
import { AssetItemViewModel } from 'src/app/models/AssetItemViewModels';
import { formatNumber } from '@angular/common';
import { BlockchainType } from 'src/app/models/BlockchainType';
import { InvoiceStatusModel } from './InvoiceStatusModel';
import { Locales } from 'src/app/constants/locales';

declare const moment: any;

@Component({
  selector: CheckoutComponent.Selector,
  templateUrl: './Checkout.html'
})
export class CheckoutComponent
  implements OnInit, OnDestroy, ICheckoutComponentHandlers {
  static readonly Selector = 'lp-checkout';

  private callbackUrl = '';
  private extendedTotalSeconds = 0;
  private extendedRemainingSeconds = 0;
  private paymentRequestId = '';
  private statusTimeout: any;

  model = new CheckoutModel();
  view = new View(this.paymentStatusCssService, this.fileService);

  constructor(
    private api: CheckoutApi,
    private assetsApi: AssetsApi,
    private confirmModalService: ConfirmModalService,
    private paymentStatusCssService: PaymentStatusCssService,
    private fileService: FileService
  ) {}

  ngOnInit(): void {
    const invoiceId = getGuidFromPath(
      location.pathname,
      `${ROUTE_INVOICE_CHECKOUT_PAGE}`
    );

    if (!invoiceId) {
      this.confirmModalService.openModal({
        title: 'Error occured',
        content: 'Invalid identifier in the path, please contact support.'
      });
      this.view.hasError = true;
      this.view.isLoading = false;
      return;
    }

    // store callback url if exist
    this.callbackUrl = getParameterByName('callback', location.href);

    this.api.getDetails(invoiceId).subscribe(
      (res: CheckoutResponse) => {
        if (!res) {
          location.href = ROUTE_ERROR_PAGE;
          return;
        }

        if (this.checkForRedirection(res.status)) {
          return;
        }

        this.applyData(res);

        this.loadPaymentAssets(res.merchantId, res.settlementAssetId);

        this.startStatusTimeout();

        this.view.isLoading = false;
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
        this.view.hasError = true;
        this.view.isLoading = false;
      }
    );
  }

  ngOnDestroy(): void {
    this.stopTimer();
    this.stopStatusTimeout();
  }

  getFile(file: FileModel): void {
    window.open(`/api/files/${file.id}/${this.model.id}`);
  }

  refreshDetails(): void {
    this.stopTimer();

    this.model.timer.seconds = 0;
    this.model.timer.mins = 0;
    this.updatePie();

    this.api.refreshDetails(this.model.id).subscribe(
      (res: CheckoutResponse) => {
        if (!res) {
          location.href = ROUTE_ERROR_PAGE;
          return;
        }

        this.applyData(res);

        this.view.isLoading = false;
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
        this.view.isLoading = false;
      }
    );
  }

  onChangedPaymentAssetId(newPaymentAssetId: string): void {
    if (!newPaymentAssetId) {
      return;
    }

    if (newPaymentAssetId !== this.model.paymentAssetId) {
      this.confirmModalService.openModal({
        content: 'Are you sure you want to change payment asset?',
        yesAction: () => {
            this.changePaymentAsset(this.model.id, newPaymentAssetId);
        },
        closeAction: () => {
            this.model.revertPaymentAssetTrigger++;
        }
    });
    }
  }

  private checkForRedirection(status: string): boolean {
    if (status === PaymentStatus[PaymentStatus.Paid] && this.callbackUrl) {
      location.href = this.callbackUrl;
      return true;
    } else if (status === PaymentStatus[PaymentStatus.Removed]) {
      location.href = ROUTE_ERROR_PAGE;
      return true;
    }

    return false;
  }

  private startStatusTimeout(): void {
    this.stopStatusTimeout();
    this.model.statusTimeout = setTimeout(() => this.updateStatus(), 15 * 1000);
  }

  private stopStatusTimeout(): void {
    if (this.model.statusTimeout) {
      clearTimeout(this.model.statusTimeout);
      this.model.statusTimeout = undefined;
    }
  }

  private updateStatus(): void {
    this.api.getStatus(this.model.id).subscribe(
      (res: InvoiceStatusModel) => {
        if (res.status !== this.model.status) {
          this.stopTimer();

          if (this.checkForRedirection(res.status)) {
            return;
          }

          this.updateDetails(false);
        } else if (res.paymentRequestId !== this.paymentRequestId) {
          this.updateDetails(true);
        }

        this.startStatusTimeout();
      },
      error => {
        console.error(error);
        this.startStatusTimeout();
      }
    );
  }

  private updateDetails(isPaymentRequestChanged: boolean) {
    this.api.getDetails(this.model.id).subscribe(
      (res: CheckoutResponse) => {
        if (!res) {
          location.href = ROUTE_ERROR_PAGE;
          return;
        }

        this.applyData(res);

        // if (isPaymentRequestChanged) {
        //   $rootScope.$broadcast('changeSelectPicker');
        // }
      },
      error => {
        console.error(error);
      }
    );
  }

  private applyData(data: CheckoutResponse): void {
    this.model.id = data.id;
    this.model.number = data.number;
    this.model.status = data.status;
    this.model.merchantId = data.merchantId;
    this.model.merchant = data.merchant;
    this.model.paymentAmount = data.paymentAmount;
    this.model.settlementAmount = data.settlementAmount;
    this.model.paymentAssetId = data.paymentAssetId;
    this.model.paymentAssetNetwork = data.paymentAssetNetwork;
    this.model.paymentAssetDisplay = data.paymentAssetDisplay;
    this.model.paymentAssetSelect = data.paymentAssetId;
    this.model.settlementAssetId = data.settlementAssetId;
    this.model.settlementAssetDisplay = data.settlementAssetDisplay;
    this.model.paymentAssetAccuracy = data.paymentAssetAccuracy;
    this.model.settlementAssetAccuracy = data.settlementAssetAccuracy;
    this.model.exchangeRate = data.exchangeRate;
    this.model.deltaSpread = data.deltaSpread;
    this.model.percents = data.percents;
    this.model.pips = data.pips;
    this.model.fee = data.fee;
    this.model.dueDate = moment(data.dueDate);
    this.model.paidAmount = data.paidAmount;
    this.model.paidDate = data.paidDate ? moment(data.paidDate) : null;
    this.model.note = data.note;
    this.model.walletAddress = data.walletAddress;
    this.model.files = data.files;

    this.view.hiddenExchangeRate =
      data.paymentAssetId === data.settlementAssetId;

    this.extendedTotalSeconds = data.extendedTotalSeconds;
    this.extendedRemainingSeconds = data.extendedRemainingSeconds;
    this.paymentRequestId = data.paymentRequestId;

    this.updateMessage(data);

    if (data.status === PaymentStatus[PaymentStatus.Unpaid]) {
      this.setQrCodeData();

      if (data.remainingSeconds > 0) {
        // timer before order.DueDate
        this.model.timer.total = data.totalSeconds;
        this.model.timer.seconds = data.remainingSeconds;
        this.model.timer.isExtended = false;
      } else {
        // timer before order.ExtendedDueDate
        this.initExtendedTimer();
      }

      this.view.isWaiting = true;

      this.restartTimer();
    } else {
      this.model.qrCodeData = '';
      this.model.timer.total = 0;
      this.model.timer.seconds = 0;
      this.view.isWaiting = false;

      this.updateHeader();
    }
  }

  private updateMessage(data: CheckoutResponse): void {
    this.model.message = '';

    const values = [];

    if (data.percents > 0) {
      values.push(
        data.percents.toLocaleString(undefined, { minimumFractionDigits: 1 }) +
          '%'
      );
    }

    if (data.pips > 0) {
      values.push(data.pips + ' pips');
    }

    if (data.fee > 0) {
      const feeText = `${formatNumber(
        data.fee,
        Locales.default,
        `1.0-${data.settlementAssetAccuracy}`
      )} ${data.settlementAssetDisplay}`;

      values.push(feeText);
    }

    let fee;

    if (values.length === 3) {
      fee = values[0] + ', ' + values[1] + ' and ' + values[2];
    } else {
      fee = values.join(' and ');
    }

    if (data.paymentAssetId === data.settlementAssetId) {
      if (data.deltaSpread && fee) {
        this.model.message = 'Includes ' + fee + ' fee of processing payment.';
      }
    } else {
      if (data.deltaSpread) {
        if (data.percents > 0 && data.pips === 0 && data.fee === 0) {
          this.model.message =
            'Includes ' + fee + ' for covering the exchange risk';
        } else if (fee) {
          this.model.message =
            'Includes ' +
            fee +
            ' uplift for covering the exchange risk and the fee of processing payment.';
        }
      } else if (fee) {
        this.model.message = 'Includes ' + fee + ' fee of processing payment.';
      }
    }
  }

  private setQrCodeData(): void {
    const labelEncoded = encodeURIComponent('invoice #' + this.model.number);

    if (this.model.paymentAssetNetwork === BlockchainType[BlockchainType.Ethereum]) {
      setEthereumQrCodeData(this.model, this.paymentRequestId);
    } else {
      setBitcoinQrCodeData(this.model, this.paymentRequestId);
    }

    function setEthereumQrCodeData(
      model: CheckoutModel,
      paymentRequestId: string
    ) {
      // ethereum:<address>[?value=<value>][?label=<label>][?message=<message>]
      model.qrCodeData = encodeURIComponent(
        'ethereum:' +
          model.walletAddress +
          '?value=' +
          model.paymentAmount +
          '&label=' +
          labelEncoded +
          '&message=' +
          paymentRequestId
      );
    }

    function setBitcoinQrCodeData(
      model: CheckoutModel,
      paymentRequestId: string
    ) {
      // bip21 for BTC https://github.com/bitcoin/bips/blob/master/bip-0021.mediawiki#examples
      // bitcoin:<address>[?amount=<amount>][?label=<label>][?message=<message>]
      model.qrCodeData = encodeURIComponent(
        'bitcoin:' +
          model.walletAddress +
          '?amount=' +
          model.paymentAmount +
          '&label=' +
          labelEncoded +
          '&message=' +
          paymentRequestId
      );
    }
  }

  private initExtendedTimer(): void {
    this.model.timer.total = this.extendedTotalSeconds;
    this.model.timer.seconds = this.extendedRemainingSeconds;
    this.model.timer.isExtended = true;
  }

  private restartTimer(): void {
    this.stopTimer();
    this.updatePie();
    this.model.timer.interval = setInterval(() => this.tick(), 1000);
  }

  private stopTimer(): void {
    if (this.model.timer.interval) {
      clearInterval(this.model.timer.interval);
      this.model.timer.interval = undefined;
    }
  }

  private updatePie(): void {
    if (this.model.timer.seconds > 0 && this.model.timer.total > 0) {
      const degs =
        360 - 360 * (this.model.timer.seconds / this.model.timer.total);
      this.model.pie.transform1 =
        'rotate(' + (degs > 180 ? 180 : degs) + 'deg) translate(0, -25%)';
      this.model.pie.transform2 =
        'rotate(' + (degs > 180 ? degs - 180 : 0) + 'deg) translate(0, -25%)';
    }
  }

  private tick(): void {
    this.model.timer.seconds--;
    // tslint:disable-next-line:no-bitwise
    this.model.timer.mins = ((this.model.timer.seconds / 60) | 0) + 1;

    if (this.model.timer.seconds <= 0) {
      if (!this.model.timer.isExtended) {
        this.initExtendedTimer();
        this.restartTimer();
      } else {
        this.refreshDetails();
      }
    } else {
      this.updatePie();
    }
  }

  updateHeader() {
    const paidAmountText = `${formatNumber(
      this.model.paidAmount,
      Locales.default,
      `1.0-${this.model.paymentAssetAccuracy}`
    )} ${this.model.paymentAssetDisplay}`;

    const dateText = this.model.paidDate ? this.model.paidDate.format('l') : '';
    const receivedDateText = ' received on ' + dateText;

    this.model.header.color = this.paymentStatusCssService.getAlertStatusCss(
      PaymentStatus[this.model.status]
    );

    switch (this.model.status) {
      case PaymentStatus[PaymentStatus.InProgress]:
        this.model.header.title = this.model.status;
        this.model.header.message = 'Payment in progress';
        this.model.header.icon = 'icon--check_circle';
        break;
      case PaymentStatus[PaymentStatus.Paid]:
        this.model.header.title = this.model.status;
        this.model.header.message = 'Invoice has been paid on ' + dateText;
        this.model.header.icon = 'icon--check_circle';
        break;
      case PaymentStatus[PaymentStatus.RefundInProgress]:
        this.model.header.title = this.model.status;
        this.model.header.message = 'Refund in progress';
        this.model.header.icon = '';
        break;
      case PaymentStatus[PaymentStatus.Refunded]:
        this.model.header.title = this.model.status;
        this.model.header.message = 'Invoice has been refunded on ' + dateText;
        this.model.header.icon = 'icon--refund';
        break;
      case PaymentStatus[PaymentStatus.Underpaid]:
        this.model.header.title = this.model.status;
        this.model.header.message = paidAmountText + receivedDateText;
        this.model.header.icon = 'icon--remove_circle';
        break;
      case PaymentStatus[PaymentStatus.Overpaid]:
        this.model.header.title = this.model.status;
        this.model.header.message = paidAmountText + receivedDateText;
        this.model.header.icon = 'icon--add_circle';
        break;
      case PaymentStatus[PaymentStatus.LatePaid]:
        this.model.header.title = this.model.status;
        this.model.header.message = paidAmountText + receivedDateText;
        this.model.header.icon = 'icon--warning_icn';
        break;
      case PaymentStatus[PaymentStatus.NotConfirmed]:
        this.model.header.title = 'Error';
        this.model.header.message = 'Transfer has not been confirmed';
        this.model.header.icon = 'icon--warning_icn';
        break;
      case PaymentStatus[PaymentStatus.InternalError]:
        this.model.header.title = 'Error';
        this.model.header.message = 'Internal error occurred';
        this.model.header.icon = 'icon--warning_icn';
        break;
      default:
        this.model.header.title = 'Error';
        this.model.header.message = 'Unknown status';
        this.model.header.icon = 'icon--warning_icn';
        this.model.header.color = 'alert--red';
        break;
    }
  }

  private loadPaymentAssets(
    merchantId: string,
    settlementAssetId: string
  ): void {
    this.view.isLoadingPaymentAssets = true;

    this.assetsApi.getPaymentAssets(merchantId, settlementAssetId).subscribe(
      (res: AssetItemViewModel[]) => {
        this.model.paymentAssets = res || [];
        this.view.isLoadingPaymentAssets = false;
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
        this.view.isLoadingPaymentAssets = false;
      }
    );
  }

  private changePaymentAsset(invoiceId: string, paymentAssetId: string): void {
    this.view.isPaymentAssetUpdating = true;
    this.stopStatusTimeout();
    this.api.changePaymentAsset(invoiceId, paymentAssetId).subscribe(
      (res: CheckoutResponse) => {
        this.applyData(res);

        this.startStatusTimeout();
        this.view.isPaymentAssetUpdating = false;
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
        this.startStatusTimeout();
        this.model.revertPaymentAssetTrigger++;
        this.view.isPaymentAssetUpdating = false;
      }
    );
  }
}

interface ICheckoutComponentHandlers {
  getFile: (_: FileModel) => void;
  refreshDetails: () => void;
  onChangedPaymentAssetId: (_: string) => void;
}

interface IViewHandlers {
  getStatusCss: (_: string) => string;
  getFileExtension: (_: string) => string;
  getFileSize: (_: number) => string;
}

class View implements IViewHandlers {
  isLoading: boolean;
  hasError: boolean;
  hiddenExchangeRate: boolean;
  isWaiting: boolean;
  isPaymentAssetUpdating: boolean;
  isLoadingPaymentAssets: boolean;

  constructor(
    private paymentStatusCssService: PaymentStatusCssService,
    private fileService: FileService
  ) {
    this.isLoading = true;
  }

  getStatusCss(status: string): string {
    return this.paymentStatusCssService.getStatusCss(PaymentStatus[status]);
  }

  getFileExtension(fileName: string): string {
    return this.fileService.getExtension(fileName);
  }

  getFileSize(fileSize: number): string {
    return this.fileService.getSize(fileSize);
  }
}
