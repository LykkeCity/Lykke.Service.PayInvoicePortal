import { Injectable } from '@angular/core';
import { PaymentStatus } from '../../models/Payment/PaymentStatus';

@Injectable()
export class PaymentStatusCssService {
  getStatusCss(status: PaymentStatus, prefix: string = null): string {
    if (!prefix) {
      prefix = 'label';
    }

    switch (status) {
      case PaymentStatus.Draft:
        return prefix + '--gray';
      case PaymentStatus.Unpaid:
        return prefix + '--yellow';
      case PaymentStatus.Removed:
        return '';
      case PaymentStatus.InProgress:
      case PaymentStatus.RefundInProgress:
        return prefix + '--blue';
      case PaymentStatus.Paid:
        return prefix + '--green';
      case PaymentStatus.Underpaid:
      case PaymentStatus.Overpaid:
      case PaymentStatus.LatePaid:
        return prefix + '--violet';
      case PaymentStatus.Refunded:
        return prefix + '--dark';
      case PaymentStatus.NotConfirmed:
      case PaymentStatus.InternalError:
      case PaymentStatus.PastDue:
        return prefix + '--red';
    }
    return '';
  }
  getAlertStatusCss(status: PaymentStatus): string {
    return this.getStatusCss(status, 'alert');
  }
}
