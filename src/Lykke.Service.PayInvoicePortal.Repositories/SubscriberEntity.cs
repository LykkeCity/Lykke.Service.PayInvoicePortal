using Lykke.Service.PayInvoicePortal.Core.Domain;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.PayInvoicePortal.Repositories
{
    public class SubscriberEntity : TableEntity, ISubscriber
    {
        public static string GeneratePartitionKey()
        {
            return "Subscriber";
        }

        public static string GenerateRowKey(string email)
        {
            return email;
        }

        public static SubscriberEntity Create(ISubscriber subscriber)
        {
            var result = new SubscriberEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(subscriber.Email)
            };

            return result;
        }

        public static SubscriberEntity Get(ISubscriber subscriber)
        {
            var result = new SubscriberEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = subscriber.Email,
            };

            return result;
        }

        public string Email => RowKey;
    }
}
