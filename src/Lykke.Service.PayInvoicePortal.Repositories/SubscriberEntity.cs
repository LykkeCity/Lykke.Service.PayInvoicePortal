using Lykke.AzureStorage.Tables;

namespace Lykke.Service.PayInvoicePortal.Repositories
{
    public class SubscriberEntity : AzureTableEntity
    {
        public SubscriberEntity()
        {
        }

        public SubscriberEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}
