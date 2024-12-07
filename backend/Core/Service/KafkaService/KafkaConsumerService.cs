using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Service.KafkaService
{
    public class KafkaConsumerService
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly string _topic;

        public KafkaConsumerService(string bootstrapServers, string topic, string groupId)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
            _topic = topic;
        }

        public void StartConsuming(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    if (consumeResult != null)
                    {
                        Console.WriteLine($"Consumed message: {consumeResult.Message.Value}");
                        // Process the message
                        ProcessMessage(consumeResult.Message.Value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
            }
            finally
            {
                _consumer.Close();
            }
        }

        private void ProcessMessage(string message)
        {
            // Implement your message processing logic here
            Console.WriteLine($"Processing message: {message}");
        }
    }
}
