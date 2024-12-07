using System.Text;
using System.Text.Json;
using Core.Service.Email;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Service.RabbitMQEmailService
{
    public class EmailConsumerService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EmailConsumerService(IConnection connection, IServiceScopeFactory serviceScopeFactory)
        {
            _connection = connection;
            _serviceScopeFactory = serviceScopeFactory;
            _channel = _connection.CreateModel();
        }

        public void Start()
        {
            string queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUENAME") ?? "email_queue";

            _channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message);

                if (emailMessage != null)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendEmailAsync(emailMessage.Email, emailMessage.Subject, emailMessage.Body);
                    }
                }
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }
    }

    public class EmailMessage
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
