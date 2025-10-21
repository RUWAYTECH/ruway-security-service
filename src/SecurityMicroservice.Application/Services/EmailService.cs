using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SecurityMicroservice.Application.IServices;
using SecurityMicroservice.Shared.DTOs;

namespace SecurityMicroservice.Application.Services
{
    public class EmailService: IEmailService
    {
        
        private readonly EmailSettings _settings;
        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mail = new MailMessage(_settings.Username, to, subject, body)
            {
                IsBodyHtml = isHtml
            };

            await client.SendMailAsync(mail);
        }
    }
}