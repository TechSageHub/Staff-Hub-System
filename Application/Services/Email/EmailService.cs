using Application.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Application.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_mailSettings.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_mailSettings.SmtpHost, _mailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(
                    _mailSettings.FromEmail,
                    _mailSettings.Password
                ),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
