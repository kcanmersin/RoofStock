using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Service.StockApi;
using Core.Data.Entity;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using Core.Service.Email; 

namespace Core.Notification.StockPriceAlert
{
    public class StockPriceAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;
        private readonly IEmailService _emailService; 

        public StockPriceAlertService(
            ApplicationDbContext context,
            IStockApiService stockApiService,
            IEmailService emailService) 
        {
            _context = context;
            _stockApiService = stockApiService;
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

                    await SendAlertEmailAsync(alert, currentPrice);

                    _context.StockPriceAlerts.Update(alert);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SendAlertEmailAsync(Core.Data.Entity.StockPriceAlert alert, decimal currentPrice)
        {
            var subject = $"Stock Price Alert: {alert.StockSymbol}";
            var body = $"Dear {alert.User.FirstName},\n\n" +
                       $"The stock {alert.StockSymbol} has {alert.AlertType.ToString().ToLower()}d to your target price of {alert.TargetPrice}. " +
                       $"The current price is {currentPrice}.\n\n";

            try
            {
                await _emailService.SendEmailAsync(alert.User.Email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email for alert {alert.Id}: {ex.Message}");
            }
        }
    }
}
