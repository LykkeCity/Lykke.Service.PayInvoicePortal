using Autofac;
using Lykke.Service.PayInvoicePortal.Core.Services;

namespace Lykke.Service.PayInvoicePortal.Services
{
    public class AutofacModule : Module
    {
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

            builder.RegisterType<BalanceService>()
                .As<IBalanceService>();
            
            builder.RegisterType<InvoiceService>()
                .As<IInvoiceService>();

            builder.RegisterType<EmployeeCache>()
                .As<IEmployeeCache>()
                .SingleInstance();
        }
    }
}
