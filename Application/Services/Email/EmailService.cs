namespace Application.Services.Email;

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Data.Model;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly MailSettings _mailSettings;

    public EmailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

   
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using var smtp = new SmtpClient
        {
            Host = _mailSettings.Host,
            Port = _mailSettings.Port,
            EnableSsl = true,
            Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

       
        smtp.TargetName = "STARTTLS/smtp.mailtrap.io";

        await smtp.SendMailAsync(message);
    }
}
