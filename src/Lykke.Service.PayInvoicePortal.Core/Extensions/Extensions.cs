using Common;

namespace Lykke.Service.PayInvoicePortal.Core.Extensions
{
    public static class Extensions
    {
        public static string ToStringNoZeros(this decimal number, int accuracy)
        {
            return number.GetFixedAsString(accuracy).TrimEnd('0').TrimEnd('.');
        }

        public static string FirstLetterUpperCase(this string src)
        {
            if (string.IsNullOrEmpty(src))
                return src;

            var firstLetter = char.ToUpperInvariant(src[0]);

            if (firstLetter == src[0])
                return src;

            return firstLetter + src.Substring(1, src.Length - 1);
        }
    }
}
