using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoicePortal.Core.Extensions;
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
            ILogFactory logFactory)
        {
            _payAuthClient = payAuthClient;
            _payInvoiceClient = payInvoiceClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task<(EmployeeModel Employee, ValidateResultModel ValidateResult)> ValidateAsync(string email, string password)
        {
            ValidateResultModel validateResult;

            try
            {
                validateResult = await _payAuthClient.ValidatePasswordAsync(email, password);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return (null, null);
            }

            if (!validateResult.Success)
            {
                _log.Warning("The e-mail or password you entered incorrect.");
                return (null, null);
            }

            try
            {
                return (await _payInvoiceClient.GetEmployeeAsync(validateResult.EmployeeId), validateResult);
            }
            catch (Exception ex)
            {
                _log.ErrorWithDetails(ex, details: validateResult.ToJson());

                return (null, null);
            }
        }
    }
}
