using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
