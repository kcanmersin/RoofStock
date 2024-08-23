using System.Threading.Tasks;

namespace Core.Service.KafkaService
{
    public class UserActivityService
    {
        private readonly KafkaProducerService _kafkaProducerService;

        public UserActivityService(KafkaProducerService kafkaProducerService)
        {
            _kafkaProducerService = kafkaProducerService;
        }

        public async Task TrackUserActivityAsync(string userId, string activity, object additionalData = null)
        {
            var message = new
            {
                UserId = userId,
                Activity = activity,
                Timestamp = System.DateTime.UtcNow,
                Data = additionalData
            };

            await _kafkaProducerService.ProduceAsync(message);
        }
    }
}
