using System;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
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

        public AutofacModule(
            IReloadingManager<AppSettings> settings,
            IServiceCollection services)
        {
            _settings = settings;

            services.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(settings.CurrentValue.AssetsServiceClient.ServiceUrl),
                settings.CurrentValue.PayInvoicePortal.AssetsCacheExpirationPeriod));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new PayInvoiceClient(_settings.CurrentValue.PayInvoiceServiceClient))
                .As<IPayInvoiceClient>()
                .SingleInstance();
            
            builder.RegisterInstance(new PayAuthClient(_settings.CurrentValue.PayAuthServiceClient))
                .As<IPayAuthClient>()
                .SingleInstance();

            builder.RegisterInstance(new PayInternalClient(_settings.CurrentValue.PayInternalServiceClient))
                .As<IPayInternalClient>()
                .SingleInstance();

            builder.RegisterInstance(new EmailPartnerRouterClient(_settings.CurrentValue.EmailPartnerRouterServiceClient.ServiceUrl))
                .As<IEmailPartnerRouterClient>()
                .SingleInstance();

            builder.RegisterRateCalculatorClient(_settings.CurrentValue.RateCalculatorServiceClient.ServiceUrl);
        }
    }
}
