using Polly;
using Polly.Timeout;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Core.Service.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly IAsyncPolicy _timeoutPolicy;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<SmtpException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} after {timeSpan.Seconds} seconds due to {exception.GetType().Name}: {exception.Message}");
                    });

            _timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10));
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("Starting to send email to {Email}", email);

            var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? _configuration["Email:Smtp:Host"];
            var portString = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? _configuration["Email:Smtp:Port"];
            var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _configuration["Email:Smtp:Username"];
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _configuration["Email:Smtp:Password"];
            var from = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _configuration["Email:Smtp:From"];

            if (!int.TryParse(portString, out int port))
            {
                _logger.LogError("Invalid SMTP port number: {Port}", portString);
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
                    _logger.LogDebug("Sending email to {Email}", email);

                    await _retryPolicy.ExecuteAsync(() =>
                        _timeoutPolicy.ExecuteAsync(async () =>
                            await client.SendMailAsync(mailMessage)));

                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
                catch (SmtpException ex)
                {
                    _logger.LogError(ex, "SMTP error occurred while sending email to {Email}", email);
                    throw;
                }
                catch (TimeoutRejectedException ex)
                {
                    _logger.LogError(ex, "Timeout occurred while sending email to {Email}", email);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error occurred while sending email to {Email}", email);
                    throw;
                }
            }
        }
    }
}
