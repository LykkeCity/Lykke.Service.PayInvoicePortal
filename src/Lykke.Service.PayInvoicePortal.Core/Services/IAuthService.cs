using System.Threading.Tasks;
using Lykke.Service.PayInvoice.Client.Models.Employee;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IAuthService
    {
        Task<EmployeeModel> ValidateAsync(string email, string password);
    }
}
