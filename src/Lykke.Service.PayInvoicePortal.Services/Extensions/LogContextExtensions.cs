using Common;
using Lykke.Service.PayInvoice.Client.Models.Invoice;

namespace Lykke.Service.PayInvoicePortal.Services.Extensions
{
    public static class LogContextExtensions
    {
        public static CreateInvoiceModel Sanitize(this CreateInvoiceModel model)
        {
            model.ClientEmail = model.ClientEmail.SanitizeEmail();
            model.ClientName = model.ClientName.Sanitize();

            return model;
        }

        public static UpdateInvoiceModel Sanitize(this UpdateInvoiceModel model)
        {
            model.ClientEmail = model.ClientEmail.SanitizeEmail();
            model.ClientName = model.ClientName.Sanitize();

            return model;
        }

        public static InvoiceModel Sanitize(this InvoiceModel model)
        {
            model.ClientEmail = model.ClientEmail.SanitizeEmail();
            model.ClientName = model.ClientName.Sanitize();

            return model;
        }

        public static string Sanitize(this string str)
        {
            str = str.SanitizeEmail();
            return str;
        }
    }
}
