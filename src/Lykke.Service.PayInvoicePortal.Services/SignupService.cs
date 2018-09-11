using System.Linq;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
using Lykke.Service.PayInvoicePortal.Core.Services;
using MoreLinq;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class SignupService : ISignupService
    {
        private const string Space = " ";

        public SignupService(
            bool enableSignup
        )
        {
            EnableSignup = enableSignup;
        }

        public bool EnableSignup { get; }

        public string GetIdFromName(string merchantName)
        {
            var splited = merchantName.ToLowerInvariant().Split(Space);

            var upperCased = splited.Select(s =>
            {
                var trimmed = s.Trim();

                return trimmed.FirstLetterUpperCase();
            });

            var id = string.Join(string.Empty, upperCased);

            return id;
        }
    }
}
