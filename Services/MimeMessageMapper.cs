using System;
using System.Linq;
using Lyralabs.TempMailServer.Data;
using MimeKit;
using Riok.Mapperly.Abstractions;

namespace Lyralabs.TempMailServer
{
    [Mapper]
    public partial class MimeMessageMapper
    {
        public MailModel MapToMailModel(MimeMessage source)
        {
            var target = new MailModel
            {
                Secret = Guid.NewGuid(),
                Subject = source.Subject,
                FromAddress = source.From.OfType<MailboxAddress>().Single().Address,
                FromName = source.From.OfType<MailboxAddress>().Single().Name,
                ReceivedDate = source.Date.UtcDateTime,
                BodyHtml = source.HtmlBody,
                BodyText = source.TextBody
            };

            return target;
        }
    }
}
