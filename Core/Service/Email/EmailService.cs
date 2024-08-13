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
            var emailSettings = _configuration.GetSection("Email:Smtp");

            if (string.IsNullOrEmpty(emailSettings["Host"]) ||
                string.IsNullOrEmpty(emailSettings["Port"]) ||
                string.IsNullOrEmpty(emailSettings["Username"]) ||
                string.IsNullOrEmpty(emailSettings["Password"]) ||
                string.IsNullOrEmpty(emailSettings["From"]))
            {
                throw new Exception("Email settings are not configured correctly.");
            }

            var client = new SmtpClient(emailSettings["Host"])
            {
                Port = int.Parse(emailSettings["Port"]),
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["From"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send email.", ex);
            }
        }
    }
}
