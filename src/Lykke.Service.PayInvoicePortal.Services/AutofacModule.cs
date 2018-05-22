using Autofac;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.AppSettings;
using Lykke.Service.PayInvoicePortal.Core.Domain.Settings.ServiceSettings;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AutofacModule : Module
    {
        private readonly CacheExpirationPeriodsSettings _cacheExpirationPeriods;
        private readonly AssetsMapSettings _assetsMap;

        public AutofacModule(
            CacheExpirationPeriodsSettings cacheExpirationPeriods,
            AssetsMapSettings assetsMap)
        {
            _cacheExpirationPeriods = cacheExpirationPeriods;
            _assetsMap = assetsMap;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<AuthService>()
                .As<IAuthService>();

            builder.RegisterType<AssetService>()
                .As<IAssetService>();

            builder.RegisterType<EmailService>()
                .As<IEmailService>();

            builder.RegisterType<InvoiceService>()
                .As<IInvoiceService>()
                .WithParameter(TypedParameter.From(_cacheExpirationPeriods));

            builder.RegisterType<EmployeeCache>()
                .As<IEmployeeCache>()
                .SingleInstance();

            builder.RegisterType<LykkeAssetsResolver>()
                .As<ILykkeAssetsResolver>()
                .WithParameter(TypedParameter.From(_assetsMap));
        }
    }
}
