using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInvoicePortal.Models.User
{
    public class SignupModel
    {
        [Required]
        [RowKey]
        [MinLength(3)]
        [MaxLength(1000)]
        public string MerchantName { get; set; }

        [Required]
        [MaxLength(255)]
        public string EmployeeFirstName { get; set; }

        [Required]
        [MaxLength(255)]
        public string EmployeeLastName { get; set; }

        [Required]
        [MaxLength(255)]
        [RowKey]
        [Email(ErrorMessage = "Invalid email")]
        public string EmployeeEmail { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(128)]
        public string EmployeePassword { get; set; }

        [Required]
        public string HostUrl { get; set; }
    }
}
