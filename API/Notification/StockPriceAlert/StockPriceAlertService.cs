using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Service.StockApi;
using Core.Data.Entity;
using System.ComponentModel;
using Core.Service.Email;

namespace API.Notification.StockPriceAlert
{
    public class StockPriceAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly StockPriceMonitorService _stockPriceMonitorService;
        private readonly IEmailService _emailService;

        public StockPriceAlertService(
            ApplicationDbContext context,
            IStockApiService stockApiService,
            StockPriceMonitorService stockPriceMonitorService,
            IEmailService emailService)
        {
            _context = context;
            _stockApiService = stockApiService;
            _stockPriceMonitorService = stockPriceMonitorService;
            _emailService = emailService;
        }

        [DisplayName("Check and Trigger Stock Price Alerts")]
        public async Task CheckAndTriggerAlertsAsync()
        {
            var pendingAlerts = await _context.StockPriceAlerts
                .Include(a => a.User)
                .Where(a => !a.IsTriggered)
                .ToListAsync();

            foreach (var alert in pendingAlerts)
            {
                await Task.Delay(5000); 

                var currentPrice = await _stockApiService.GetStockPriceAsync(alert.StockSymbol);

                bool shouldTrigger = false;

                if (alert.AlertType == AlertType.Rise && currentPrice >= alert.TargetPrice)
                {
                    shouldTrigger = true;
                }
                else if (alert.AlertType == AlertType.Fall && currentPrice <= alert.TargetPrice)
                {
                    shouldTrigger = true;
                }

                if (shouldTrigger)
                {
                    alert.IsTriggered = true;
                    alert.TriggeredDate = DateTime.UtcNow;

                    await _stockPriceMonitorService.SendStockPriceAlertAsync(alert.UserId.ToString(), alert.StockSymbol, currentPrice);

                    //var user = alert.User;
                    //if (user != null && !string.IsNullOrEmpty(user.Email))
                    //{
                     //   var subject = $"Stock Price Alert: {alert.StockSymbol}";
                     //   var message = $"Dear {user.FirstName},\n\nThe stock {alert.StockSymbol} has {alert.AlertType.ToString().ToLower()}d to your target price of {alert.TargetPrice}. The current price is {currentPrice}.\n\n";

                      //  await _emailService.SendEmailAsync(user.Email, subject, message);
                    //}

                    _context.StockPriceAlerts.Update(alert);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
