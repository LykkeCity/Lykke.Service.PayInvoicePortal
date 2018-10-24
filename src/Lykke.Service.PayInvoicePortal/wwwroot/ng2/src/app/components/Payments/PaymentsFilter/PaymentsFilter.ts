import { Component, Input, Output, EventEmitter } from '@angular/core';
import { PaymentsFilterModel } from './PaymentsFilterModel';
import { UserService } from 'src/app/services/UserService';
import { PaymentsFilterLocalStorageKeys } from './PaymentsFilterLocalStorageKeys';

@Component({
  selector: PaymentsFilterComponent.Selector,
  templateUrl: './PaymentsFilter.html'
})
export class PaymentsFilterComponent implements IPaymentFilterHandlers {
  static readonly Selector = 'lp-payments-filter';

  @Input('filter')
  model: PaymentsFilterModel;

  @Output()
  filterChanged = new EventEmitter<PaymentsFilterModel>();

  constructor(private userService: UserService) {}

  private firstChange = {
    period: true,
    type: true,
    status: true
  };

  onChangedPeriod(period): void {
    if (!this.model.isFilterInitialized) {
      return;
    }

    if (this.firstChange.period) {
      this.firstChange.period = false;
      return;
    }

    localStorage.setItem(
      PaymentsFilterLocalStorageKeys.Period(this.userService.user),
      period
    );

    this.emitFilterChanged();
  }

  onChangedType(type): void {
    if (!this.model.isFilterInitialized) {
      return;
    }

    if (this.firstChange.type) {
      this.firstChange.type = false;
      return;
    }

    localStorage.setItem(
      PaymentsFilterLocalStorageKeys.Type(this.userService.user),
      type
    );

    this.emitFilterChanged();
  }

  onChangedStatus(status): void {
    if (!this.model.isFilterInitialized) {
      return;
    }

    if (this.firstChange.status) {
      this.firstChange.status = false;
      return;
    }

    localStorage.setItem(
      PaymentsFilterLocalStorageKeys.Status(this.userService.user),
      status
    );

    this.emitFilterChanged();
  }

  onChangedSearch(searchText): void {
    if (!this.model.isFilterInitialized) {
      return;
    }

    localStorage.setItem(
      PaymentsFilterLocalStorageKeys.SearchText(this.userService.user),
      searchText
    );
    this.emitFilterChanged();
  }

  clearSearchText(): void {
    if (this.model.searchText) {
      this.model.searchText = '';
      localStorage.setItem(
        PaymentsFilterLocalStorageKeys.SearchText(this.userService.user),
        ''
      );
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
