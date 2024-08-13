using System.Threading.Tasks;
using API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace API.Notification.StockPriceAlert
{
    public class StockPriceMonitorService
    {
        private readonly IHubContext<StockPriceHub> _hubContext;

        public StockPriceMonitorService(IHubContext<StockPriceHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendStockPriceAlertAsync(string userId, string stockSymbol, decimal currentPrice)
        {
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveStockPriceAlert", stockSymbol, currentPrice);
        }
    }
}
