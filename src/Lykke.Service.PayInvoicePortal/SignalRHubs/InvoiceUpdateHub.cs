using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.PayInvoicePortal.Extensions;
using Lykke.Service.PayInvoicePortal.RabbitSubscribers;
using Microsoft.AspNetCore.SignalR;

namespace Lykke.Service.PayInvoicePortal.SignalRHubs
{
    public class InvoiceUpdateHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.GetMerchantId());
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.GetMerchantId());
            await base.OnDisconnectedAsync(exception);
        }
    }
}
