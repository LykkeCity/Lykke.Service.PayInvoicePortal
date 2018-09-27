import { Component, Input } from '@angular/core';
import { AssetModel } from '../../../models/AssetModel';

@Component({
  selector: PaymentsStatisticComponent.Selector,
  templateUrl: './PaymentsStatistic.html'
})
export class PaymentsStatisticComponent {
  static readonly Selector = 'lp-payments-statistic';
  @Input()
  baseAsset: AssetModel;
}
