import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import {
  PaymentsFilterModel,
  PaymentsFilterLocalStorageKeys
} from './PaymentsFilterModel';

@Component({
  selector: PaymentsFilterComponent.Selector,
  templateUrl: './PaymentsFilter.html'
})
export class PaymentsFilterComponent implements OnInit, IPaymentFilterHandlers {
  static readonly Selector = 'lp-payments-filter';

  @Input('filter')
  model: PaymentsFilterModel;

  @Output()
  filterChanged = new EventEmitter<PaymentsFilterModel>();

  private firstChange = {
    period: true,
    type: true,
    status: true
  };

  ngOnInit(): void {}

  onChangedPeriod(period): void {
    if (this.firstChange.period) {
      this.firstChange.period = false;
      return;
    }

    localStorage.setItem(PaymentsFilterLocalStorageKeys.Period, period);

    this.emitFilterChanged();
  }

  onChangedType(type): void {
    if (this.firstChange.type) {
      this.firstChange.type = false;
      return;
    }

    localStorage.setItem(PaymentsFilterLocalStorageKeys.Type, type);

    this.emitFilterChanged();
  }

  onChangedStatus(status): void {
    if (this.firstChange.status) {
      this.firstChange.status = false;
      return;
    }

    localStorage.setItem(PaymentsFilterLocalStorageKeys.Status, status);

    this.emitFilterChanged();
  }

  onChangedSearch(searchText): void {
    localStorage.setItem(PaymentsFilterLocalStorageKeys.SearchText, searchText);

    this.emitFilterChanged();
  }

  clearSearchText(): void {
    if (this.model.searchText) {
      this.model.searchText = '';
      localStorage.setItem(PaymentsFilterLocalStorageKeys.SearchText, '');
      this.emitFilterChanged();
    }
  }

  private emitFilterChanged(): void {
    if (this.model && this.filterChanged) {
      this.filterChanged.emit(this.model);
    }
  }
}

interface IPaymentFilterHandlers {
  onChangedPeriod: (_: any) => void;
  onChangedType: (_: any) => void;
  onChangedStatus: (_: any) => void;
  onChangedSearch: (_: any) => void;
  clearSearchText: () => void;
}
