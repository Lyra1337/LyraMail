using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyralabs.TempMailServer.Data.Entities
{
    public class MailModel : ModelBase
    {
        public Guid Secret { get; } = Guid.NewGuid();
        public string Subject { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string BodyHtml { get; set; }
        public string BodyText { get; set; }
        public DateTime ReceivedDate { get; set; }

        public int MailboxId { get; set; }

        [ForeignKey(nameof(MailboxId))]
        public MailboxModel Mailbox { get; set; }
    }
}
