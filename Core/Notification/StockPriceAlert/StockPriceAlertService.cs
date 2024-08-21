using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Service.StockApi;
using Core.Data.Entity;
using System.ComponentModel;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace Core.Notification.StockPriceAlert
{
    internal class StockPriceAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;

        public StockPriceAlertService(
            ApplicationDbContext context,
            IStockApiService stockApiService)
        {
            _context = context;
            _stockApiService = stockApiService;
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

                    PublishAlertToQueue(alert, currentPrice);

                    _context.StockPriceAlerts.Update(alert);
                }
            }

            await _context.SaveChangesAsync();
        }

        private void PublishAlertToQueue(Core.Data.Entity.StockPriceAlert alert, decimal currentPrice)
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            string queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUENAME") ?? "email_queue";

            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = new
            {
                //Email = alert.User.Email,
                Email ="kcanmersin@gmail.com",
                Subject = $"Stock Price Alert: {alert.StockSymbol}",
                Body = $"Dear {alert.User.FirstName},\n\nThe stock {alert.StockSymbol} has {alert.AlertType.ToString().ToLower()}d to your target price of {alert.TargetPrice}. The current price is {currentPrice}.\n\n"
            };

            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
