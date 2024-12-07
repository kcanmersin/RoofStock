using Confluent.Kafka;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Service.KafkaService
{
    public class KafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducerService(string bootstrapServers, string topic)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _topic = topic;
        }

        public async Task ProduceAsync<T>(T message)
        {
            var messageBody = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = messageBody };

            await _producer.ProduceAsync(_topic, kafkaMessage);
        }
    }
}
