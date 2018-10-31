using System;
using Autofac;
using Common;
using Lykke.Service.Assets.Client;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoicePortal.RabbitSubscribers;
using Lykke.Service.PayInvoicePortal.Services;
using Lykke.Service.PayInvoicePortal.Settings;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.PayInvoicePortal
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public AutofacModule(
            IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl),
                _settings.CurrentValue.PayInvoicePortal.AssetsCacheExpirationPeriod));

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

            builder.RegisterPayMerchantClient(_settings.CurrentValue.PayMerchantServiceClient, null);

            builder.RegisterType<RealtimeNotificationsService>()
                .As<IRealtimeNotificationsService>()
                .SingleInstance();

            RegisterRabbitSubscribers(builder);
        }

        private void RegisterRabbitSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<InvoiceUpdateSubscriber>()
                .AsSelf()
                .As<IStartable>()
                .As<IStopable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PayInvoicePortal.Rabbit));
        }
    }
}
