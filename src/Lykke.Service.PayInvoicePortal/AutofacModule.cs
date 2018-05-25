using System;
using Autofac;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.Settings;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PayInvoicePortal
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public AutofacModule(IReloadingManager<AppSettings> settings, IServiceCollection services, ILog log)
        {
            _settings = settings;
            _log = log;

            services.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(settings.CurrentValue.AssetsServiceClient.ServiceUrl),
                settings.CurrentValue.PayInvoicePortal.AssetsCacheExpirationPeriod));
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

            builder.RegisterInstance(new EmailPartnerRouterClient(_settings.CurrentValue.EmailPartnerRouterServiceClient.ServiceUrl))
                .As<IEmailPartnerRouterClient>()
                .SingleInstance();

            builder.RegisterInstance(new BalancesClient(_settings.CurrentValue.BalancesServiceClient.ServiceUrl, _log))
                .As<IBalancesClient>()
                .SingleInstance();

            builder.RegisterInstance(new RateCalculatorClient(_settings.CurrentValue.RateCalculatorServiceClient.ServiceUrl, _log))
                .As<IRateCalculatorClient>()
                .SingleInstance();
        }
    }
}
