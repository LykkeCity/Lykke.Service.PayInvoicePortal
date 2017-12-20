using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Core;

namespace Lykke.Pay.Invoice.AppCode
{
    public static class StringExt
    {

       
        private static readonly IFormatProvider Provider = CultureInfo.InvariantCulture;


        public static DateTime GetRepoDateTime(this string strDate)
        {
            return DateTime.Parse(strDate, Provider).ToLocalTime();
        }

        
        public static DateTime FromUnixFormat(this string str)
        {
            try
            {
                int seconds = int.Parse(str);
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).ToLocalTime();
                
            }
            catch
            {
                return DateTime.Now;
            }

        }

        public static MerchantPayRequestStatus ParseOrderStatus(this string status)
        {
            int e;
            MerchantPayRequestStatus result;
            if (int.TryParse(status, out e))
            {
                result = (MerchantPayRequestStatus)e;
            }
            else
            {
                result = Enum.Parse<MerchantPayRequestStatus>(status);
            }

            return result;
        }
    }
}
