using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Client.Models.Invoice;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IPayInvoiceClient _payInvoiceClient;
        private readonly ILog _log;

        public InvoiceService(IPayInvoiceClient payInvoiceClient, ILog log)
        {
            _payInvoiceClient = payInvoiceClient;
            _log = log;
        }

        public async Task<IReadOnlyList<InvoiceModel>> GetAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending)
        {
            IReadOnlyList<InvoiceModel> result =
                await GetAsync(merchantId, period, searchValue, sortField, sortAscending);

            if (status.Count > 0)
            {
                result = result
                    .Where(o => status.Contains(o.Status))
                    .ToList();
            }

            return result;
        }

        public async Task<InvoiceSource> GetAsync(
            string merchantId,
            IReadOnlyList<InvoiceStatus> status,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending,
            int skip,
            int take)
        {
            IReadOnlyList<InvoiceModel> result =
                await GetAsync(merchantId, period, searchValue, sortField, sortAscending);

            var source = new InvoiceSource
            {
                Total = result.Count,
                CountPerStatus = new Dictionary<InvoiceStatus, int>()
            };

            if (status.Count > 0)
            {
                source.Items = result
                    .Where(o => status.Contains(o.Status))
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }
            else
            {
                source.Items = result
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            }

            foreach (InvoiceStatus value in Enum.GetValues(typeof(InvoiceStatus)))
                source.CountPerStatus[value] = result.Count(o => o.Status == value);

            return source;
        }

        private async Task<IReadOnlyList<InvoiceModel>> GetAsync(
            string merchantId,
            Period period,
            string searchValue,
            string sortField,
            bool sortAscending)
        {
            IEnumerable<InvoiceModel> invoices = await _payInvoiceClient.GetInvoicesAsync(merchantId);

            var query = invoices.AsQueryable();

            if (!string.IsNullOrEmpty(sortField))
            {
                query = sortAscending ? query.OrderBy(sortField) : query.OrderBy($"{sortField} descending");
            }
            else
            {
                query = query.OrderByDescending(o => o.CreatedDate);
            }

            if (period != Period.AllTime)
            {
                DateTime dateFrom = DateTime.Today;

                if (period == Period.CurrentMonth)
                    dateFrom = DateTime.Now.Date.AddDays(-DateTime.Now.Day);

                if (period == Period.LastMonth)
                    dateFrom = DateTime.Now.Date.AddDays(-DateTime.Now.Day).AddMonths(-1);

                if (period == Period.LastThreeMonths)
                    dateFrom = DateTime.Now.Date.AddDays(-DateTime.Now.Day).AddMonths(-3);

                query = query.Where(i => i.DueDate >= dateFrom);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(o => (o.ClientEmail ?? string.Empty).ToLower().Contains(searchValue) ||
                                         (o.Number ?? string.Empty).ToLower().Contains(searchValue));
            }

            return query.ToList();
        }
    }
}
