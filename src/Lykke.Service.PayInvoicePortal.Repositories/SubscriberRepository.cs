using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInvoicePortal.Core.Domain;
using Lykke.Service.PayInvoicePortal.Core.Repositories;

namespace Lykke.Service.PayInvoicePortal.Repositories
{
    public class SubscriberRepository : ISubscriberRepository
    {
        private readonly INoSQLTableStorage<SubscriberEntity> _tableStorage;

        public SubscriberRepository(INoSQLTableStorage<SubscriberEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<Subscriber> GetAsync(string email)
        {
            SubscriberEntity entity = await _tableStorage.GetDataAsync(GetPartitionKey(), GetRowKey(email));

            if (entity == null)
                return null;

            return new Subscriber
            {
                Email = entity.RowKey
            };
        }

        public async Task InsertAsync(Subscriber subscriber)
        {
            await _tableStorage.InsertAsync(new SubscriberEntity(GetPartitionKey(), GetRowKey(subscriber.Email)));
        }

        private static string GetPartitionKey()
            => "Subscriber";

        private static string GetRowKey(string email)
            => email.ToLower();
    }
}
