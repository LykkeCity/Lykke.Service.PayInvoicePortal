import { PaymentModel } from '../../models/Payment/PaymentModel';

export class PaymentsResponse {
  payments: PaymentModel[];
  hasMorePayments: boolean;
  hasAnyPayment: boolean;
}
