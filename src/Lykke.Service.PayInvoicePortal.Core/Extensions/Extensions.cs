using Common;

namespace Lykke.Service.PayInvoicePortal.Core.Extensions
{
    public static class Extensions
    {
        public static string ToStringNoZeros(this decimal number, int accuracy)
        {
            return number.GetFixedAsString(accuracy).TrimEnd('0').TrimEnd('.');
        }
    }
}
