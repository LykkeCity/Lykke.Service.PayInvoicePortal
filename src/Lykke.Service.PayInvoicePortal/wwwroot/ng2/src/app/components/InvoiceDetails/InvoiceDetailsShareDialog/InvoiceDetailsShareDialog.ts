import {
  Component,
  Input,
  Output,
  EventEmitter,
  ElementRef,
  ViewChild,
  AfterViewInit,
  NgZone
} from '@angular/core';
import { EmailApi } from 'src/app/services/api/EmailApi';
import { SendInvoiceEmailRequest } from './SendInvoiceEmailRequest';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
  FormControl
} from '@angular/forms';
import { nameof, isValidEmail } from 'src/app/utils/utils';

declare const $: any;

@Component({
  selector: InvoiceDetailsShareDialogComponent.Selector,
  templateUrl: './InvoiceDetailsShareDialog.html'
})
export class InvoiceDetailsShareDialogComponent implements AfterViewInit {
  static readonly Selector = 'lp-invoice-details-share-dialog';

  @Input()
  show: boolean;

  @Input()
  invoiceNumber: string;

  @Input()
  invoiceId: string;

  @Input()
  invoiceCheckoutUrl: string;

  @Output()
  closeDialog = new EventEmitter();

  @ViewChild('emailsElement')
  emailsElement: ElementRef;

  shareForm: FormGroup;
  emails: AbstractControl;
  emailsValue: string[] = [];
  isLoading: boolean;
  isError: boolean;
  isSuccess: boolean;

  constructor(private zone: NgZone, fb: FormBuilder, private api: EmailApi) {
    this.shareForm = fb.group({
      [nameof(() => this.emails)]: ['']
    });
    this.emails = this.shareForm.controls[nameof(() => this.emails)];
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.initTags();
    });
  }

  private initTags(opts?: { isRefresh?: boolean }) {
    $(this.emailsElement.nativeElement).amsifySuggestags(
      {
        isValidFunc: value => this.isValidEmail(value),
        afterAdd: value => this.afterAdd(value),
        afterRemove: value => this.afterRemove(value)
      },
      opts && opts.isRefresh ? 'refresh' : null
    );
  }

  private isValidEmail(value): boolean {
    return isValidEmail(value);
  }

  private afterAdd(value): void {
    this.zone.run(() => {
      this.emailsValue.push(value);
      this.emails.markAsTouched();
      this.emails.setErrors(
        this.emails.hasError('anyInvalid') || !isValidEmail(value)
          ? { anyInvalid: true }
          : null
      );
    });
  }

  private afterRemove(value): void {
    this.zone.run(() => {
      this.emailsValue.splice(this.emailsValue.indexOf(value), 1);

      if (this.emailsValue.length) {
        this.emails.setErrors(
          this.hasInvalidEmail(this.emailsValue) ? { anyInvalid: true } : null
        );
      } else {
        this.emails.setErrors({
          required: true
        });
      }
    });
  }

  send(): void {
    this.isLoading = true;
    this.isError = false;
    this.isSuccess = false;

    const model = new SendInvoiceEmailRequest(
      this.invoiceId,
      this.invoiceCheckoutUrl,
      this.emailsValue
    );

    this.api.sendInvoiceEmail(model).subscribe(
      res => {
        this.isLoading = false;
        this.isSuccess = true;

        // clear emails
        this.emailsValue = [];
        this.emails.reset();
        this.initTags({ isRefresh: true });
      },
      error => {
        console.error(error);
        this.isError = true;
        this.isLoading = false;
      }
    );
  }

  close(): void {
    this.isSuccess = false;
    this.closeDialog.emit(null);
  }

  overlayClick(e: MouseEvent): void {
    if (this.show) {
      e.preventDefault();
      e.stopPropagation();

      this.close();
    }
  }

  private hasInvalidEmail(emails: string[]): boolean {
    return emails.some(value => !isValidEmail(value));
  }
}
