using System;
using System.Collections.Generic;

namespace Lyralabs.Net.TempMailServer
{
    public class EmailDto
    {
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public List<MailboxDto> To { get; set; }
        public string Message { get; set; }
    }
}
