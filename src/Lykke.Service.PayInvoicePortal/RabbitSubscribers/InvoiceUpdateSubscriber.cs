using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInvoice.Contract.Invoice;
using Lykke.Service.PayInvoicePortal.Services;
using Lykke.Service.PayInvoicePortal.Settings.ServiceSettings;

namespace Lykke.Service.PayInvoicePortal.RabbitSubscribers
{
    public class InvoiceUpdateSubscriber : IStartable, IStopable
    {
        private readonly IRealtimeNotificationsService _realtimeNotificationsService;
        private readonly RabbitSettings _rabbitSettings;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        private RabbitMqSubscriber<InvoiceUpdateMessage> _subscriber;

        public InvoiceUpdateSubscriber(
            IRealtimeNotificationsService realtimeNotificationsService,
            RabbitSettings rabbitSettings,
            ILogFactory logFactory)
        {
            _realtimeNotificationsService = realtimeNotificationsService;
            _rabbitSettings = rabbitSettings;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var rabbitMqSubscriptionSettings = RabbitMqSubscriptionSettings
                .ForSubscriber(_rabbitSettings.ConnectionString, 
                    _rabbitSettings.InvoiceUpdateExchangeName,
                    nameof(PayInvoicePortal))
                .MakeDurable();

            rabbitMqSubscriptionSettings.DeadLetterExchangeName = null;

            _subscriber = new RabbitMqSubscriber<InvoiceUpdateMessage>(
                    _logFactory,
                    rabbitMqSubscriptionSettings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        rabbitMqSubscriptionSettings,
                        TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<InvoiceUpdateMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();

            _log.Info($"{nameof(InvoiceUpdateSubscriber)} is started.");
        }

        private Task ProcessMessageAsync(InvoiceUpdateMessage message)
        {
            return _realtimeNotificationsService.SendInvoiceUpdateAsync(message);
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber.Stop();
        }
    }
}
