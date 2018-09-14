using System;
using System.Linq;
using System.Security.Claims;

namespace Lykke.Service.PayInvoicePortal.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetEmployeeId(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(u => u.Type == ClaimTypes.Sid).Value;
        }

        public static string GetMerchantId(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(u => u.Type == ClaimTypes.UserData).Value;
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(u => u.Type == ClaimTypes.Email).Value;
        }

        public static string GetName(this ClaimsPrincipal principal)
        {
            var name = principal.Claims.First(u => u.Type == ClaimTypes.Name).Value;

            return name;
        }

        public static bool IsSupervisor(this ClaimsPrincipal principal)
        {
            if (principal.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Actor) == null)
                return false;
            return Convert.ToBoolean(principal.Claims.First(u => u.Type == ClaimTypes.Actor).Value);
        }
    }
}
