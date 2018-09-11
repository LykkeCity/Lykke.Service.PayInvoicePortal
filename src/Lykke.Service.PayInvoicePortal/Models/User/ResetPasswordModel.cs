using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.User
{
    public class ResetPasswordModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
