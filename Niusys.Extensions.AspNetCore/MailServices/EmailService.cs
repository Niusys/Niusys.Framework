using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Niusys.Extensions.AspNetCore.MailServices
{
    public class EmailService
    {
        private readonly EmailSetting _emailConfiguration;

        public EmailService(IOptions<EmailSetting> emailConfigurationOptions)
        {
            if (emailConfigurationOptions is null)
            {
                throw new System.ArgumentNullException(nameof(emailConfigurationOptions));
            }

            _emailConfiguration = emailConfigurationOptions.Value;
        }
        public async Task SendAsync(EmailMessage emailMessage)
        {
            if (emailMessage is null)
            {
                throw new System.ArgumentNullException(nameof(emailMessage));
            }

            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            var sender = emailMessage.FromAddresses.First();
            message.Sender = new MailboxAddress(sender.Name, sender.Address);
            message.Subject = emailMessage.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };


            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
#pragma warning disable CA5359 // 请勿禁用证书验证
                emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
#pragma warning restore CA5359 // 请勿禁用证书验证
                              //The last parameter here is to use SSL (Which you should!)
                await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, SecureSocketOptions.Auto);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                await emailClient.SendAsync(message);

                await emailClient.DisconnectAsync(true);
            }
        }
    }
}
