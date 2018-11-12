import { Component, Renderer2, ViewChild } from '@angular/core';
import { InvoiceModel } from '../InvoiceDetails/InvoiceModel';
import { ROUTE_INVOICE_CHECKOUT_PAGE } from 'src/app/constants/routes';
import { FormGroup } from '@angular/forms';
import { EmailApi } from 'src/app/services/api/EmailApi';
import { SendInvoiceEmailRequest } from '../InvoiceDetails/InvoiceDetailsShareDialog/SendInvoiceEmailRequest';

declare const moment: any;

@Component({
  selector: InvoiceInfoComponent.Selector,
  templateUrl: './InvoiceInfo.html'
})
export class InvoiceInfoComponent implements IInvoiceInfoComponentHandlers {
  static readonly Selector = 'lp-invoice-info';

  @ViewChild('shareForm')
  shareForm: FormGroup;

  model = new InvoiceInfoModel();
  view = new View();
  validation = new Validation();

  constructor(private emailApi: EmailApi, private renderer: Renderer2) {}

  open(model: InvoiceModel): void {
    this.renderer.setStyle(document.body, 'overflow', 'hidden');
    this.view.show = true;

    model.dueDate = moment(model.dueDate);
    model.createdDate = moment(model.createdDate);

    this.model = model as InvoiceInfoModel;
    this.model.url = this.getCheckoutUrl(model.id);
    this.model.shareEmail = model.clientEmail;
  }

  close(): void {
    this.renderer.removeStyle(document.body, 'overflow');
    this.view.show = false;
    this.reset();
  }

  overlayClick(e: MouseEvent): void {
    if (this.view.show) {
      e.preventDefault();
      e.stopPropagation();

      this.close();
    }
  }

  share(): void {
    if (this.shareForm.invalid) {
      return;
    }

    const model = new SendInvoiceEmailRequest(
      this.model.id,
      this.getCheckoutUrl(this.model.id),
      this.model.shareEmail.split(';')
    );

    this.emailApi.sendInvoiceEmail(model).subscribe(
      res => {
        this.view.isSending = false;
        this.view.sent = true;
        this.model.shareEmail = '';
      },
      error => {
        console.error(error);
        this.validation.hasError = true;
        this.view.isSending = false;
      }
    );
  }

  private getCheckoutUrl(invoiceId: string) {
    return `${location.origin}${ROUTE_INVOICE_CHECKOUT_PAGE}/${invoiceId}`;
  }

  private reset(): void {
    this.model = null;
    this.view.reset();
    this.validation.reset();
  }
}

class InvoiceInfoModel extends InvoiceModel {
  public url = '';
  public shareEmail = '';
}

interface IInvoiceInfoComponentHandlers {
  open: (_: InvoiceModel) => void;
  close: () => void;
  overlayClick: (_: MouseEvent) => void;
  share: () => void;
}

class View {
  show: boolean;
  sent: boolean;
  isSending: boolean;

  reset(): void {
    this.sent = false;
    this.isSending = false;
  }
}

class Validation {
  hasError: boolean;

  reset(): void {
    this.hasError = false;
  }
}
