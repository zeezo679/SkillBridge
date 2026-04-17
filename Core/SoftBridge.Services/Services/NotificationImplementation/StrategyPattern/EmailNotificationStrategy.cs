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
            if (string.IsNullOrWhiteSpace(ContentDto.Email))
                return;

            // 1. prepare message
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Soft Bridge", _emailSettings.Email));

            // 2. for design body
            emailMessage.To.Add(new MailboxAddress("", ContentDto.Email));

            // 3. for design subject
            emailMessage.Subject = ContentDto.Subject;

            // 4. for design body
            emailMessage.Body = new TextPart("plain")
            {
                Text = ContentDto.Body
            };

            // 5. send emails
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
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
