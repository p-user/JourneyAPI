using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.IServices;

namespace BusinessLayer.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHTML)
        {
            string MailServer = _config["EmailSettings:MailServer"];
            string FromEmail = _config["EmailSettings:FromEmail"];
            string Password = _config["EmailSettings:Password"];
            int Port = int.Parse(_config["EmailSettings:MailPort"]);
            var client = new SmtpClient(MailServer, Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
            MailMessage mailMessage = new MailMessage(FromEmail, toEmail, subject, body)
            {
                IsBodyHtml = isBodyHTML
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}
