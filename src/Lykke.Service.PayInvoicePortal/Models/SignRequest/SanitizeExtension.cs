namespace Lykke.Service.PayInvoicePortal.Models.SignRequest
{
    public static class SanitizeExtension
    {
        public static SignRequestModel Sanitize(this SignRequestModel model)
        {
            model.RsaPrivateKey = null;
            return model;
        }
    }
}
