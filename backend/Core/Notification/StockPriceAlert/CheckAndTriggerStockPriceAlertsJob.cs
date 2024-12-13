using Quartz;
using Core.Notification.StockPriceAlert;
using System.Threading.Tasks;

namespace Core.Notification.StockPriceAlert
{
    public class CheckAndTriggerStockPriceAlertsJob : IJob
    {
        private readonly StockPriceAlertService _stockPriceAlertService;

        public CheckAndTriggerStockPriceAlertsJob(StockPriceAlertService stockPriceAlertService)
        {
            _stockPriceAlertService = stockPriceAlertService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _stockPriceAlertService.CheckAndTriggerAlertsAsync();
        }
    }
}
