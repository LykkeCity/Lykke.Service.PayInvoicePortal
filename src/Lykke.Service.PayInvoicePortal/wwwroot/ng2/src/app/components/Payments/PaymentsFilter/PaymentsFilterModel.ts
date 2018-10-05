import { IItemViewModel } from '../../../models/IItemViewModel';
import { PaymentType } from '../../../models/Payment/PaymentType';
import { PaymentStatus } from '../../../models/Payment/PaymentStatus';

export class PaymentsFilterModel {
  period: number;
  periods: IItemViewModel[];
  type: number;
  types: IItemViewModel[];
  status: string;
  statuses: PaymentsFilterStatus[];
  searchText: string;
  constructor() {
    this.period = PaymentsFilterPeriod.ThisMonth;
    this.periods = [
      { id: PaymentsFilterPeriod.ThisWeek, title: 'This week' },
      { id: PaymentsFilterPeriod.LastWeek, title: 'Last week' },
      { id: PaymentsFilterPeriod.ThisMonth, title: 'This month' },
      { id: PaymentsFilterPeriod.LastMonth, title: 'Last month' },
      { id: PaymentsFilterPeriod.ThreeMonths, title: 'Three months' },
      { id: PaymentsFilterPeriod.ThisYear, title: 'This year' },
      { id: PaymentsFilterPeriod.LastYear, title: 'Last year' }
    ];
    this.type = PaymentType.All;
    this.types = [
      { id: PaymentType.All, title: 'All' },
      { id: PaymentType.Invoice, title: 'Invoice' },
      { id: PaymentType.Api, title: 'API' }
    ];
    this.status = '';
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
}

export class PaymentsFilterLocalStorageKeys {
  static readonly Period: string = 'PaymentsFilter_Period';
  static readonly Type: string = 'PaymentsFilter_Type';
  static readonly Status: string = 'PaymentsFilter_Status';
  static readonly SearchText: string = 'PaymentsFilter_SearchText';
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
