import { Component, Input } from '@angular/core';
import { AssetModel } from '../../../models/AssetModel';

@Component({
  selector: PaymentsBalanceComponent.Selector,
  templateUrl: './PaymentsBalance.html'
})
export class PaymentsBalanceComponent {
  static readonly Selector = 'lp-payments-balance';
  @Input()
  baseAsset: AssetModel;
}
