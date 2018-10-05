export enum PaymentStatus {
  Draft = 1,
  Unpaid,
  Removed,
  InProgress,
  Paid,
  Underpaid,
  Overpaid,
  LatePaid,
  RefundInProgress,
  Refunded,
  NotConfirmed,
  InternalError,
  PastDue
}
