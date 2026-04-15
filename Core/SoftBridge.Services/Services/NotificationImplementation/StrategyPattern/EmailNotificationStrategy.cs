using SoftBridge.Shared.Common.Dto.Notification;
using Microsoft.Extensions.Options;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Dto.Notification.Settings;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace SoftBridge.Services.Services.NotificationImplementation.StrategyPattern
{
    public class EmailNotificationStrategy(IOptions<EmailSettingsDto> emailSettings) : INotificationStrategy
    {
        private readonly EmailSettingsDto _emailSettings = emailSettings.Value;
        public NotificationType Type => NotificationType.Email;

        public async Task DeliverAsync(NotificationContentDto ContentDto)
        {
            // 1. prepare message
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Soft Bridge", _emailSettings.Email));
            emailMessage.To.Add(new MailboxAddress("", ContentDto.To)); // the reciver 
            emailMessage.Subject = ContentDto.Subject;

            // 2. for design body
            emailMessage.Body = new TextPart("plain")
            {
                Text = ContentDto.Body
            };

            // 3.send emails
            using var client = new SmtpClient();
            try
            {
                // Connect to the SMTP server
                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);

                // Authenticate with the email server
                await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);

                // Send the email
                await client.SendAsync(emailMessage);
            }
            finally
            {
                // close connections
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}
