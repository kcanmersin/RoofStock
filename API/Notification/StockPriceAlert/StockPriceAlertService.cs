using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Service.StockApi;

namespace API.Notification.StockPriceAlert
{
    public class StockPriceAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly StockPriceMonitorService _stockPriceMonitorService;

        public StockPriceAlertService(ApplicationDbContext context, IStockApiService stockApiService, StockPriceMonitorService stockPriceMonitorService)
        {
            _context = context;
            _stockApiService = stockApiService;
            _stockPriceMonitorService = stockPriceMonitorService;
        }

        public async Task CheckAndTriggerAlertsAsync()
        {
            var pendingAlerts = await _context.StockPriceAlerts
                .Include(a => a.User)
                .Where(a => !a.IsTriggered)
                .ToListAsync();

            foreach (var alert in pendingAlerts)
            {
                var currentPrice = await _stockApiService.GetStockPriceAsync(alert.StockSymbol);

                if (currentPrice >= alert.TargetPrice)
                {
                    alert.IsTriggered = true;
                    alert.TriggeredDate = DateTime.UtcNow;

                    await _stockPriceMonitorService.CheckStockPriceAsync(alert.StockSymbol, alert.TargetPrice);

                    _context.StockPriceAlerts.Update(alert);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
