using Polly;
using Polly.Timeout;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Core.Service.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly IAsyncPolicy _timeoutPolicy;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            _retryPolicy = Policy
                .Handle<SmtpException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: 3, 
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) => 
                    {
                        Console.WriteLine($"Retry {retryCount} after {timeSpan.Seconds} seconds due to {exception.GetType().Name}");
                    });

            _timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10));
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? _configuration["Email:Smtp:Host"];
            var portString = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? _configuration["Email:Smtp:Port"];
            var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _configuration["Email:Smtp:Username"];
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _configuration["Email:Smtp:Password"];
            var from = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _configuration["Email:Smtp:From"];

            if (!int.TryParse(portString, out int port))
            {
                throw new Exception($"Invalid SMTP port number: {portString}");
            }

            using (var client = new SmtpClient(host))
            {
                client.Port = port;
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(from),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                try
                {
                    // Combine the timeout and retry policies
                    await _retryPolicy.ExecuteAsync(() =>
                        _timeoutPolicy.ExecuteAsync(async () => 
                            await client.SendMailAsync(mailMessage)));
                }
                catch (SmtpException ex)
                {
                    throw new Exception("SMTP error occurred while sending email.", ex);
                }
                catch (TimeoutRejectedException ex)
                {
                    throw new Exception("Timeout error occurred while sending email.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to send email.", ex);
                }
            }
        }
    }
}
