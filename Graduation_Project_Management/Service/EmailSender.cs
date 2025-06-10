using Domain.Entities;
using Graduation_Project_Management.IServices;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Graduation_Project_Management.Service
{
    public class EmailSender :IEmailSenderService
    {
        private readonly EmailSettings _emailSettings;
        public EmailSender(IOptions<EmailSettings> options)
        {
            _emailSettings = options.Value;
        }

        public async System.Threading.Tasks.Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart(TextFormat.Html) { Text = body };

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

