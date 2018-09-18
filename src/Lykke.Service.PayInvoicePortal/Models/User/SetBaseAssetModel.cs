using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInvoicePortal.Models.User
{
    public class SetBaseAssetModel
    {
        [Required]
        public string BaseAssetId { get; set; }
    }
}
