using System;
using System.Collections.Generic;
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

        public async Task<ISubscriber> GetAsync(string email)
        {
            var partitionKey = SubscriberEntity.GeneratePartitionKey();
            var rowKey = email;

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<ISubscriber>> GetAllAsync()
        {
            var partitionKey = SubscriberEntity.GeneratePartitionKey();
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<ISubscriber> CreateAsync(ISubscriber subscriber)
        {
            var partitionKey = SubscriberEntity.GeneratePartitionKey();
            var rowKey = SubscriberEntity.GenerateRowKey(subscriber.Email);

            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            if (entity != null) throw new Exception("Email exists: " + subscriber.Email);

            var newEntity = SubscriberEntity.Create(subscriber);
            await _tableStorage.InsertAsync(newEntity);

            return newEntity;
        }

        public async Task DeleteAsync(string email)
        {
            var partitionKey = SubscriberEntity.GeneratePartitionKey();
            var rowKey = email;

            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);
            await _tableStorage.DeleteAsync(entity.PartitionKey, entity.RowKey);
        }
    }
}
