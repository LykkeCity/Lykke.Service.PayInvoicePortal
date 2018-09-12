namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface ISignupService
    {
        bool EnableSignup { get; }

        string GetIdFromName(string merchantName);
    }
}
