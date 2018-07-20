using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PayInvoicePortal.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.PayInvoicePortal.Repositories
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _subscriptionConnectionString;

        public AutofacModule(
            IReloadingManager<string> subscriptionConnectionString)
        {
            _subscriptionConnectionString = subscriptionConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
                new SubscriberRepository(
                    AzureTableStorage<SubscriberEntity>.Create(_subscriptionConnectionString, "LykkeSubscribers", c.Resolve<ILogFactory>())))
                .As<ISubscriberRepository>()
                .SingleInstance();
        }
    }
}
