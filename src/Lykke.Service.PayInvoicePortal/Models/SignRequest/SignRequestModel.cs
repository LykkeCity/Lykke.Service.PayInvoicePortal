using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.SignRequest
{
    public class SignRequestModel
    {
        [Required]
        public string LykkeMerchantId { get; set; }
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string Body { get; set; }
    }
}
