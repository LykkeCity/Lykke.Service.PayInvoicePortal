using System.Threading.Tasks;
using Lykke.Service.PayAuth.Client.Models.Employees;
using Lykke.Service.PayInvoice.Client.Models.Employee;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IAuthService
    {
        Task<(EmployeeModel Employee, ValidateResultModel ValidateResult)> ValidateAsync(string email, string password);
    }
}
