using System.Collections.Concurrent;
using Lykke.Service.PayInvoice.Client.Models.Employee;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class EmployeeCache : IEmployeeCache
    {
        private readonly ConcurrentDictionary<string, EmployeeModel> _cache =
            new ConcurrentDictionary<string, EmployeeModel>();

        public EmployeeModel Get(string employeeId)
        {
            if (_cache.ContainsKey(employeeId))
                return _cache[employeeId];

            return null;
        }

        public void Set(EmployeeModel model)
        {
            _cache[model.Id] = model;
        }
    }
}
