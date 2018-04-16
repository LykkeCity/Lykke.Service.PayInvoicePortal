using Lykke.Service.PayInvoice.Client.Models.Employee;

namespace Lykke.Service.PayInvoicePortal.Core.Services
{
    public interface IEmployeeCache
    {
        EmployeeModel Get(string employeeId);
        void Set(EmployeeModel model);
    }
}
