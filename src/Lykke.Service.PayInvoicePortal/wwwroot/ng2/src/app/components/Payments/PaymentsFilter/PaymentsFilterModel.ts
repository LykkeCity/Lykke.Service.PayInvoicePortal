import { IItemViewModel } from '../../../models/IItemViewModel';
import { PaymentType } from '../../../models/Payment/PaymentType';
import { PaymentStatus } from '../../../models/Payment/PaymentStatus';

export class PaymentsFilterModel {
  period: number;
  periodDefaultValue: number;
  periods: IItemViewModel[];
  type: number;
  typeDefaultValue: number;
  types: IItemViewModel[];
  status: string;
  statusDefaultValue: string;
  statuses: PaymentsFilterStatus[];
  searchText: string;
  searchTextDefaultValue: string;
  isFilterInitialized: boolean;
  constructor() {
    this.periodDefaultValue = PaymentsFilterPeriod.ThisMonth;
    this.typeDefaultValue = PaymentType.All;
    this.statusDefaultValue = '';
    this.searchTextDefaultValue = '';

    this.periods = [
      { id: PaymentsFilterPeriod.ThisWeek, title: 'This week' },
      { id: PaymentsFilterPeriod.LastWeek, title: 'Last week' },
      { id: PaymentsFilterPeriod.ThisMonth, title: 'This month' },
      { id: PaymentsFilterPeriod.LastMonth, title: 'Last month' },
      { id: PaymentsFilterPeriod.ThreeMonths, title: 'Three months' },
      { id: PaymentsFilterPeriod.ThisYear, title: 'This year' },
      { id: PaymentsFilterPeriod.LastYear, title: 'Last year' }
    ];

    this.types = [
      { id: PaymentType.All, title: 'All' },
      { id: PaymentType.Invoice, title: 'Invoice' },
      { id: PaymentType.Api, title: 'API' }
    ];

    this.statuses = [
      {
        id: '',
        title: 'All',
        values: []
      },
      {
        id: PaymentStatus.Draft,
        title: PaymentStatus[PaymentStatus.Draft],
        values: [PaymentStatus[PaymentStatus.Draft]]
      },
      {
        id: PaymentStatus.Unpaid,
        title: PaymentStatus[PaymentStatus.Unpaid],
        values: [PaymentStatus[PaymentStatus.Unpaid]]
      },
      {
        id: PaymentStatus.InProgress,
        title: 'In Progress',
        values: [
          PaymentStatus[PaymentStatus.InProgress],
          PaymentStatus[PaymentStatus.RefundInProgress]
        ]
      },
      {
        id: PaymentStatus.Paid,
        title: PaymentStatus[PaymentStatus.Paid],
        values: [PaymentStatus[PaymentStatus.Paid]]
      },
      {
        id: PaymentStatus.Underpaid,
        title: PaymentStatus[PaymentStatus.Underpaid],
        values: [PaymentStatus[PaymentStatus.Underpaid]]
      },
      {
        id: PaymentStatus.Overpaid,
        title: PaymentStatus[PaymentStatus.Overpaid],
        values: [PaymentStatus[PaymentStatus.Overpaid]]
      },
      {
        id: PaymentStatus.LatePaid,
        title: PaymentStatus[PaymentStatus.LatePaid],
        values: [PaymentStatus[PaymentStatus.LatePaid]]
      },
      {
        id: PaymentStatus.Refunded,
        title: PaymentStatus[PaymentStatus.Refunded],
        values: [PaymentStatus[PaymentStatus.Refunded]]
      },
      {
        id: PaymentStatus.PastDue,
        title: PaymentStatus[PaymentStatus.PastDue],
        values: [PaymentStatus[PaymentStatus.PastDue]]
      },
      {
        id: PaymentStatus.Removed,
        title: PaymentStatus[PaymentStatus.Removed],
        values: [PaymentStatus[PaymentStatus.Removed]]
      },
      {
        id: PaymentStatus.InternalError,
        title: 'Error',
        values: [
          PaymentStatus[PaymentStatus.InternalError],
          PaymentStatus[PaymentStatus.NotConfirmed]
        ]
      }
    ];
  }

  initPeriod() {
    this.period = this.periodDefaultValue;
  }

  initType() {
    this.type = this.typeDefaultValue;
  }

  initStatus() {
    this.status = this.statusDefaultValue;
  }

  initSearchText() {
    this.searchText = this.searchTextDefaultValue;
  }
}

class PaymentsFilterStatus implements IItemViewModel {
  id: string | number;
  title: string;
  values: string[];
}

enum PaymentsFilterPeriod {
  All = 0,
  ThisWeek,
  LastWeek,
  ThisMonth,
  LastMonth,
  ThreeMonths,
  ThisYear,
  LastYear
}
