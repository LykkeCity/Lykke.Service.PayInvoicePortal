using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Services.Extensions
{
    public static class StatusExtensions
    {
        public static (PaymentRequestStatus PaymentRequestStatus, PaymentRequestProcessingError[] ProcessingErrors)
            ToPaymentRequestStatus(
                this InvoiceStatus invoiceStatus)
        {
            switch (invoiceStatus)
            {
                case InvoiceStatus.Unpaid:
                    return (PaymentRequestStatus.New, null);

                case InvoiceStatus.Removed:
                    return (PaymentRequestStatus.Cancelled, null);

                case InvoiceStatus.InProgress:
                    return (PaymentRequestStatus.InProcess, null);

                case InvoiceStatus.Paid:
                    return (PaymentRequestStatus.Confirmed, null);

                case InvoiceStatus.RefundInProgress:
                    return (PaymentRequestStatus.RefundInProgress, null);

                case InvoiceStatus.Refunded:
                    return (PaymentRequestStatus.Refunded, null);

                case InvoiceStatus.PastDue:
                    return (PaymentRequestStatus.Error, new[] {PaymentRequestProcessingError.PaymentExpired});

                case InvoiceStatus.LatePaid:
                    return (PaymentRequestStatus.Error, new[] {PaymentRequestProcessingError.LatePaid});

                case InvoiceStatus.Underpaid:
                    return (PaymentRequestStatus.Error, new[] {PaymentRequestProcessingError.PaymentAmountBelow});

                case InvoiceStatus.Overpaid:
                    return (PaymentRequestStatus.Error, new[] {PaymentRequestProcessingError.PaymentAmountAbove});

                case InvoiceStatus.NotConfirmed:
                    return (PaymentRequestStatus.Error, new[] {PaymentRequestProcessingError.RefundNotConfirmed});

                case InvoiceStatus.InternalError:
                    return (PaymentRequestStatus.Error,
                        new[]
                        {
                            PaymentRequestProcessingError.UnknownPayment, PaymentRequestProcessingError.UnknownRefund
                        });

                default:
                    return (PaymentRequestStatus.None, null);
            }
        }

        public static InvoiceStatus ToInvoiceStatus(this PaymentRequestStatus status, PaymentRequestProcessingError error)
        {
            switch (status)
            {
                case PaymentRequestStatus.New:
                    return InvoiceStatus.Unpaid;

                case PaymentRequestStatus.Cancelled:
                    return InvoiceStatus.Removed;

                case PaymentRequestStatus.InProcess:
                    return InvoiceStatus.InProgress;

                case PaymentRequestStatus.Confirmed:
                    return InvoiceStatus.Paid;

                case PaymentRequestStatus.RefundInProgress:
                    return InvoiceStatus.RefundInProgress;

                case PaymentRequestStatus.Refunded:
                    return InvoiceStatus.Refunded;

                case PaymentRequestStatus.Error:
                    switch (error)
                    {
                        case PaymentRequestProcessingError.PaymentExpired:
                            return InvoiceStatus.PastDue;
                        case PaymentRequestProcessingError.LatePaid:
                            return InvoiceStatus.LatePaid;
                        case PaymentRequestProcessingError.PaymentAmountBelow:
                            return InvoiceStatus.Underpaid;
                        case PaymentRequestProcessingError.PaymentAmountAbove:
                            return InvoiceStatus.Overpaid;
                        case PaymentRequestProcessingError.RefundNotConfirmed:
                            return InvoiceStatus.NotConfirmed;
                        case PaymentRequestProcessingError.UnknownPayment:
                        case PaymentRequestProcessingError.UnknownRefund:
                            return InvoiceStatus.InternalError;
                        default:
                            return InvoiceStatus.None;
                    }

                default:
                    return InvoiceStatus.None;
            }
        }
    }
}
