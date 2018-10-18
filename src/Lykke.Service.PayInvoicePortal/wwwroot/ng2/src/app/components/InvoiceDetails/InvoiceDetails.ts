import { Component, OnInit, NgZone, ViewChild } from '@angular/core';
import { ConfirmModalService } from '../../services/ConfirmModalService';
import { InvoicesApi } from '../../services/api/InvoicesApi';
import { InvoiceDetailsResponse } from './InvoiceDetailsResponse';
import { InvoiceModel } from './InvoiceModel';
import { PaymentStatus } from '../../models/Payment/PaymentStatus';
import { BlockchainType } from '../../models/BlockchainType';
import {
  ROUTE_INVOICE_DETAILS_PAGE,
  ROUTE_PAYMENTS_PAGE
} from '../../constants/routes';
import { getGuidFromPath } from '../../utils/utils';
import { PaymentStatusCssService } from '../../services/Payment/PaymentStatusCssService';
import { FileModel } from '../../models/FileModel';
import { FileService } from '../../services/FileService';
import { InvoiceDetailsRefundDialogComponent } from './InvoiceDetailsRefundDialog/InvoiceDetailsRefundDialog';

declare const pubsubEvents: any;
declare const moment: any;

@Component({
  selector: InvoiceDetailsComponent.Selector,
  templateUrl: './InvoiceDetails.html'
})
export class InvoiceDetailsComponent
  implements OnInit, IInvoiceDetailsHandlers, IShareDialog, IRefundDialog {
  static readonly Selector = 'lp-invoice-details';
  model = new InvoiceModel();

  view = new View(this.paymentStatusCssService, this.fileService);
  showRefundDialog: boolean;
  showShareDialog: boolean;

  @ViewChild(InvoiceDetailsRefundDialogComponent)
  refundDialog: InvoiceDetailsRefundDialogComponent;

  onInvoiceUpdated(): void {
    this.zone.run(() => {
      this.loadInvoice();
    });
  }

  constructor(
    private api: InvoicesApi,
    private confirmModalService: ConfirmModalService,
    private paymentStatusCssService: PaymentStatusCssService,
    private fileService: FileService,
    private zone: NgZone
  ) {}

  ngOnInit(): void {
    if ((window as any).pubsubEvents) {
      pubsubEvents.on('invoiceUpdated', () => this.onInvoiceUpdated());
    }

    const invoiceId = getGuidFromPath(
      location.pathname,
      `${ROUTE_INVOICE_DETAILS_PAGE}`
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

    this.api.getDetails(invoiceId).subscribe(
      (res: InvoiceDetailsResponse) => {
        this.view.blockchainExplorerUrl = res.blockchainExplorerUrl;
        this.view.ethereumBlockchainExplorerUrl =
          res.ethereumBlockchainExplorerUrl;

        this.applyData(res.invoice);

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

  getFile(file: FileModel): void {
    window.open(`/api/files/${file.id}/${this.model.id}`);
  }

  openRefundDialog(): void {
    this.refundDialog.loadRefundData();
    this.showRefundDialog = true;
  }

  closeRefundDialog(): void {
    this.showRefundDialog = false;
  }

  refresh(): void {
    this.loadInvoice();
  }

  share(): void {
    this.showShareDialog = true;
  }

  closeShareDialog(): void {
    this.showShareDialog = false;
  }

  edit(): void {
    if (!this.view.canEdit) {
      return;
    }

    pubsubEvents.emit('invoiceDraftEdit', {
      id: this.model.id,
      status: this.model.status,
      number: this.model.number,
      client: this.model.clientName,
      email: this.model.clientEmail,
      settlementAsset: this.model.settlementAsset,
      amount: this.model.amount,
      dueDate: this.model.dueDate,
      note: this.model.note,
      files: this.model.files
    });
  }

  delete(): void {
    if (!this.view.canDelete) {
      return;
    }

    this.confirmModalService.openModal({
      content: `Are you sure you want to remove this invoice #${
        this.model.number
      }?`,
      yesAction: () => {
        this.view.isLoading = true;

        this.api.deleteInvoice(this.model.id).subscribe(
          res => {
            switch (this.model.status) {
              case PaymentStatus[PaymentStatus.Unpaid]:
                this.loadInvoice();
                break;
              default:
                location.href = ROUTE_PAYMENTS_PAGE;
                break;
            }
          },
          error => {
            console.error(error);
            this.confirmModalService.showErrorModal();
            this.view.isLoading = false;
          }
        );
      }
    });
  }

  private loadInvoice(): void {
    this.view.isLoading = true;

    this.api.getInvoice(this.model.id).subscribe(
      (res: InvoiceModel) => {
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

  private applyData(invoice: InvoiceModel) {
    this.model = new InvoiceModel(
      invoice.id,
      invoice.number,
      invoice.clientName,
      invoice.clientEmail,
      invoice.amount,
      moment(invoice.dueDate),
      invoice.status,
      invoice.settlementAsset,
      invoice.settlementAssetDisplay,
      invoice.settlementAssetAccuracy,
      invoice.paymentAsset,
      invoice.paymentAssetNetwork,
      invoice.paymentRequestId,
      invoice.walletAddress,
      moment(invoice.createdDate),
      invoice.note
    );

    this.model.files = invoice.files;

    invoice.history.forEach(item => {
      item.dueDate = moment(item.dueDate);
      item.date = moment(item.date);
      if (item.paidDate) {
        item.paidDate = moment(item.paidDate);
      }
    });

    this.model.history = invoice.history;

    this.view.invoiceCheckoutUrl = `${location.origin}/invoice/${
      this.model.id
    }`;
    this.view.encodedInvoiceCheckoutUrl = encodeURIComponent(
      this.view.invoiceCheckoutUrl
    );

    this.view.canPay =
      this.model.status === PaymentStatus[PaymentStatus.Unpaid];
    this.view.canRefund =
      this.model.walletAddress &&
      [
        PaymentStatus[PaymentStatus.Underpaid],
        PaymentStatus[PaymentStatus.Overpaid],
        PaymentStatus[PaymentStatus.LatePaid]
      ].indexOf(this.model.status) > -1;
    this.view.canRefresh =
      this.model.status !== PaymentStatus[PaymentStatus.Draft] &&
      this.model.status !== PaymentStatus[PaymentStatus.Removed];
    this.view.canShare =
      this.model.status === PaymentStatus[PaymentStatus.Unpaid];
    this.view.canEdit =
      this.model.status === PaymentStatus[PaymentStatus.Draft];
    this.view.canDelete =
      this.model.status === PaymentStatus[PaymentStatus.Draft] ||
      this.model.status === PaymentStatus[PaymentStatus.Unpaid];
    this.view.showBcnLink =
      this.model.walletAddress &&
      [
        PaymentStatus[PaymentStatus.InProgress],
        PaymentStatus[PaymentStatus.Paid],
        PaymentStatus[PaymentStatus.Underpaid],
        PaymentStatus[PaymentStatus.Overpaid],
        PaymentStatus[PaymentStatus.LatePaid],
        PaymentStatus[PaymentStatus.Refunded],
        PaymentStatus[PaymentStatus.NotConfirmed],
        PaymentStatus[PaymentStatus.InternalError]
      ].indexOf(this.model.status) > -1;

    this.view.isEthereumPaymentAsset =
      this.model.paymentAssetNetwork.indexOf(
        BlockchainType[BlockchainType.Ethereum]
      ) > -1;
  }
}

interface IInvoiceDetailsHandlers {
  getFile: (_: FileModel) => void;
  refresh: () => void;
  share: () => void;
  edit: () => void;
  delete: () => void;
}

interface IViewHandlers {
  getInitials: (_: string) => string;
  getStatusCss: (_: string) => string;
  getFileExtension: (_: string) => string;
  getFileSize: (_: number) => string;
}

interface IRefundDialog {
  showRefundDialog: boolean;
  openRefundDialog: () => void;
  closeRefundDialog: () => void;
}

interface IShareDialog {
  showShareDialog: boolean;
  closeShareDialog: () => void;
}

class View implements IViewHandlers {
  isLoading: boolean;
  hasError: boolean;
  blockchainExplorerUrl: string;
  ethereumBlockchainExplorerUrl: string;
  invoiceCheckoutUrl: string;
  encodedInvoiceCheckoutUrl: string;
  canPay: boolean;
  canRefund: boolean;
  canRefresh: boolean;
  canShare: boolean;
  canEdit: boolean;
  canDelete: boolean;
  showBcnLink: boolean;
  isEthereumPaymentAsset: boolean;

  constructor(
    private paymentStatusCssService: PaymentStatusCssService,
    private fileService: FileService
  ) {
    this.isLoading = true;
  }

  getInitials(author: string): string {
    let value = '';

    if (!author || author.length === 0) {
      return value;
    }

    const parts = author.split(' ');

    if (parts.length > 0) {
      value = value + parts[0][0].toUpperCase();
    }

    if (parts.length > 1) {
      value = value + parts[1][0].toUpperCase();
    }

    return value;
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
