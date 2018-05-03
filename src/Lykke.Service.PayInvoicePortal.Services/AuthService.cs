using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPayAuthClient _payAuthClient;
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public AuthService(
            IPayAuthClient payAuthClient,
            IPayInvoiceClient payInvoiceClient,
            ILog log)
        {
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        public async Task<EmployeeModel> ValidateAsync(string email, string password)
        {
            ValidateResultModel response;

            try
            {
                response = await _payAuthClient.ValidateAsync(email, password);
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(ValidateAsync), new {email = email.SanitizeEmail()}, exception);
                return null;
            }

            if (!response.Success)
            {
                _log.WriteWarning(nameof(ValidateAsync), new {email = email.SanitizeEmail()},
                    "The e-mail or password you entered incorrect.");
                return null;
            }

            try
            {
                return await _payInvoiceClient.GetEmployeeAsync(response.EmployeeId);
            }
            catch (Exception exception)
            {
                _log.WriteError(nameof(ValidateAsync), new
                    {
                        response.MerchantId,
                        response.EmployeeId
                    },
                    exception);

                return null;
            }
        }
    }
}
