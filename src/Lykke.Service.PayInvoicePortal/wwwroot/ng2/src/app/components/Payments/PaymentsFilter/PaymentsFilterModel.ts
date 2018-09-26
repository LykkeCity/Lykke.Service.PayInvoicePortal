import { IItemViewModel } from '../../../models/IItemViewModel';

export class PaymentsFilterModel {
  period: number;
  periods: IItemViewModel[];
  type: string;
  types: IItemViewModel[];
  status: string;
  statuses: PaymentsFilterStatus[];
  search: string;
  constructor() {
    this.period = 0;
    this.periods = [
      { id: 0, title: 'All time' },
      { id: 1, title: 'This month' },
      { id: 2, title: 'Last month' },
      { id: 3, title: 'Three month ago' }
    ];
    this.type = '';
    this.types = [
      { id: '', title: 'All' },
      { id: 'invoice', title: 'Invoice' },
      { id: 'api', title: 'API' }
    ];
    this.status = '';
    this.statuses = [
      { id: '', title: 'All', values: [] },
      { id: 'draft', title: 'Draft', values: ['Draft'] },
      { id: 'unpaid', title: 'Unpaid', values: ['Unpaid'] },
      {
        id: 'inProgress',
        title: 'In Progress',
        values: ['InProgress', 'RefundInProgress']
      },
      { id: 'paid', title: 'Paid', values: ['Paid'] },
      { id: 'underpaid', title: 'Underpaid', values: ['Underpaid'] },
      { id: 'overpaid', title: 'Overpaid', values: ['Overpaid'] },
      { id: 'latePaid', title: 'LatePaid', values: ['LatePaid'] },
      { id: 'refunded', title: 'Refunded', values: ['Refunded'] },
      { id: 'pastDue', title: 'PastDue', values: ['PastDue'] },
      { id: 'removed', title: 'Removed', values: ['Removed'] },
      {
        id: 'error',
        title: 'Error',
        values: ['InternalError', 'InvalidAddress', 'NotConfirmed']
      }
    ];
  }
}

class PaymentsFilterStatus implements IItemViewModel {
  id: string;
  title: string;
  values: string[];
}
