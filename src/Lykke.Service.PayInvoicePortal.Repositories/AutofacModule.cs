using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.PayInvoicePortal.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.PayInvoicePortal.Repositories
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _subscriptionConnectionString;
        private readonly ILog _log;

        public AutofacModule(IReloadingManager<string> subscriptionConnectionString, ILog log)
        {
            _subscriptionConnectionString = subscriptionConnectionString;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance<ISubscriberRepository>(
                new SubscriberRepository(
                    AzureTableStorage<SubscriberEntity>.Create(_subscriptionConnectionString,
                        "LykkeSubscribers", _log))
            ).SingleInstance();
        }
    }
}
