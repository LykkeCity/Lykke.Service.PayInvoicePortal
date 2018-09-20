using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.User
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
