using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class StockPriceHub : Hub
    {
        public async Task SendMessageToUser(string userId, string message)
        {
            await Clients.Group(userId).SendAsync("ReceiveMessage", message);
        }
    }

}