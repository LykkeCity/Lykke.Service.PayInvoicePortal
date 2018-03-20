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

            builder.RegisterType<InvoiceService>()
                .As<IInvoiceService>();
        }
    }
}
