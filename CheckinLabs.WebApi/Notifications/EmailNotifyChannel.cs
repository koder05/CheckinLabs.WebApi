using CheckinLabs.BL.Enum;
using CheckinLabs.BL.Svc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckinLabs.WebApi.Notifications
{
    class EmailNotifyChannel : INotifyChannel
    {
        private readonly SmtpConfig smtpConf;
        public NotifyChannelType Type => NotifyChannelType.Email;
        public EmailNotifyChannel(IConfiguration cfg)
        {
            smtpConf = cfg.GetSection(nameof(SmtpConfig)).Get<SmtpConfig>();
        }
        public async Task SendNotificationAsync(string addr, string header, string msgTmp, IDictionary<string, string> tmpParams)
        {
            if (smtpConf == null)
                throw new Exception("Smtp configuration missed");
            var message = new MimeMessage();
            message.To.AddRange(new MailboxAddress[] { new MailboxAddress(addr, addr) });
            message.From.AddRange(new MailboxAddress[] { new MailboxAddress(smtpConf.User, smtpConf.Server) });

            message.Subject = header;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = msgTmp
            };

            using (var emailClient = new MailKit.Net.Smtp.SmtpClient())
            {
                emailClient.Connect(smtpConf.Server, smtpConf.Port, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(smtpConf.User, smtpConf.Pass);
                await emailClient.SendAsync(message);
                emailClient.Disconnect(true);
            }
        }
    }
}
