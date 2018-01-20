using System.ComponentModel.DataAnnotations;

namespace Lykke.Pay.Invoice.Models.Auth
{
    public class SignInViewModel
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
       
    }
}
