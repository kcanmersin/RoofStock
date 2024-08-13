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

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? _configuration["Email:Smtp:Host"];
            var portString = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? _configuration["Email:Smtp:Port"];
            var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _configuration["Email:Smtp:Username"];
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _configuration["Email:Smtp:Password"];
            var from = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _configuration["Email:Smtp:From"];

            Console.WriteLine($"Host: {host}");
            Console.WriteLine($"Port: {portString}");
            Console.WriteLine($"Username: {username}");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"From: {from}");

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
                    await client.SendMailAsync(mailMessage);
                }
                catch (SmtpException ex)
                {
                    throw new Exception("SMTP error occurred while sending email.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to send email.", ex);
                }
            }
        }
    }
}
