using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Models.User
{
    public class UserInfoModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeEmail { get; set; }
        public string FullName { get; set; }
        public string MerchantId { get; set; }
    }
}
