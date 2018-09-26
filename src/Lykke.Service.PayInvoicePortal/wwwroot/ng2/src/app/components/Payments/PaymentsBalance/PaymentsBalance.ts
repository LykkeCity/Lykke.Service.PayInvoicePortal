import { Component, Input } from '@angular/core';

@Component({
  selector: PaymentsBalanceComponent.Selector,
  templateUrl: './PaymentsBalance.html'
})
export class PaymentsBalanceComponent {
  static readonly Selector = 'lp-payments-balance';
  @Input()
  baseAsset: string;
}
