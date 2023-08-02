using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lyralabs.TempMailServer.Data
{
    public class MailModel : ModelBase
    {
        public Guid Secret { get; set; }
        public string Subject { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string BodyHtml { get; set; }
        public string Password { get; set; }
        public bool IsRead { get; set; }

        public MailModel Clone()
        {
            return (MailModel)this.MemberwiseClone();
        }

        public string BodyText { get; set; }
        public DateTime ReceivedDate { get; set; }

        public int MailboxId { get; set; }

        [ForeignKey(nameof(MailboxId))]
        public MailboxModel Mailbox { get; set; }
    }
}
