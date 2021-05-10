using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyralabs.TempMailServer
{
    public class MailboxDto
    {
        public string Address { get; set; }

        public string PublicKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<EmailDto> Mails { get; set; } = new List<EmailDto>();
    }
}
