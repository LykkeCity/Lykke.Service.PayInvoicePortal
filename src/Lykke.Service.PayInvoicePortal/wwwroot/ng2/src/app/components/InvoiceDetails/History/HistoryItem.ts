import { HistoryItemModel } from './HistoryItemModel';
import { Input, Component } from '@angular/core';
import { PaymentStatus } from '../../../models/Payment/PaymentStatus';
import { PaymentStatusCssService } from '../../../services/Payment/PaymentStatusCssService';

@Component({
  selector: HistoryItemComponent.Selector,
  templateUrl: './HistoryItem.html'
})
export class HistoryItemComponent implements IHistoryItemHandlers {
  static readonly Selector = 'lp-history-item';

  @Input('historyItem')
  model: HistoryItemModel;

  view = new View(this.paymentStatusCssService);

  PaymentStatus = PaymentStatus;

  constructor(private paymentStatusCssService: PaymentStatusCssService) {}

  getInitials(author: string) {
    let initials = '';

    if (!author || author.length === 0) {
      return initials;
    }

    const parts = author.split(' ');

    if (parts.length > 0) {
      initials = parts[0][0].toUpperCase();
    }

    if (parts.length > 1) {
      initials = initials + parts[1][0].toUpperCase();
    }

    return initials;
  }
}

interface IHistoryItemHandlers {
  getInitials: (_: string) => string;
}

interface IViewHandlers {
  getStatusCss: (_: string) => string;
}

class View implements IViewHandlers {
  constructor(private paymentStatusCssService: PaymentStatusCssService) {}
  getStatusCss(status: string): string {
    return this.paymentStatusCssService.getStatusCss(PaymentStatus[status]);
  }
}
