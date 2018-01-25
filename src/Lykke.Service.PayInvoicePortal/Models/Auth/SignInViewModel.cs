using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.Auth
{
    public class SignInViewModel
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
