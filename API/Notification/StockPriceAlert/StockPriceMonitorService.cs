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

        public async Task CheckStockPriceAsync(string stockSymbol, decimal targetPrice)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveStockPriceAlert", stockSymbol, targetPrice);
        }
    }
}
