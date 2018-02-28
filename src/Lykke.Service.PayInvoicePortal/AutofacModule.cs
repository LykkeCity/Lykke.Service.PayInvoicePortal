using Autofac;
using Common.Log;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.PayInvoicePortal
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public AutofacModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();
            
            builder.RegisterInstance(new PayInvoiceClient(_settings.CurrentValue.PayInvoiceServiceClient))
                .As<IPayInvoiceClient>()
                .SingleInstance();
            
            builder.RegisterInstance(new PayAuthClient(_settings.CurrentValue.PayAuthServiceClient, _log))
                .As<IPayAuthClient>()
                .SingleInstance();

            builder.RegisterInstance(new PayInternalClient(_settings.CurrentValue.PayInternalServiceClient))
                .As<IPayInternalClient>()
                .SingleInstance();
        }
    }
}
