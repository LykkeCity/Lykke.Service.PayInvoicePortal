namespace Lykke.Pay.Invoice.Clients.MerchantAuth.Models
{
    public class MerchantStaffSignInResponse
    {
        public string MerchantId { get; set; }
        public string MerchantStaffEmail { get; set; }
        public string MerchantStaffFirstName { get; set; }
        public string MerchantStaffLastName { get; set; }
        public string MerchantStaffPassword { get; set; }
        public string LwId { get; set; }
    }
}
