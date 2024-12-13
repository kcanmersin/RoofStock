using Quartz;
using Core.Service.OrderBackgroundService;
using System.Threading.Tasks;

namespace Core.Service.OrderBackgroundService
{
    public class CheckAndProcessOrdersJob : IJob
    {
        private readonly OrderBackgroundService _orderBackgroundService;

        public CheckAndProcessOrdersJob(OrderBackgroundService orderBackgroundService)
        {
            _orderBackgroundService = orderBackgroundService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _orderBackgroundService.CheckAndProcessOrders();
        }
    }
}
