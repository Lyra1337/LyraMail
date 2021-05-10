using System;
using System.Collections.Generic;

namespace Lyralabs.TempMailServer
{
    public class EmailDto
    {
        public Guid Secret { get; } = Guid.NewGuid();
        public string Subject { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public List<MailboxAddressDto> To { get; set; }
        public string BodyHtml { get; set; }
        public string BodyText { get; set; }
        public DateTime ReceivedDate { get; set; }

        public EmailDto Clone() => (EmailDto)this.MemberwiseClone();
    }
}
