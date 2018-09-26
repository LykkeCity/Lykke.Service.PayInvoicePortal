import { Component, Input, OnInit, Output } from '@angular/core';
import { EventEmitter } from 'selenium-webdriver';
import { PaymentsFilterModel } from './PaymentsFilterModel';

@Component({
  selector: PaymentsFilterComponent.Selector,
  templateUrl: './PaymentsFilter.html'
})
export class PaymentsFilterComponent implements OnInit {
  static readonly Selector = 'lp-payments-filter';

  model = new PaymentsFilterModel();

  @Output()
  filterChanged: EventEmitter<PaymentsFilterModel> = new EventEmitter();

  ngOnInit() {}
}
