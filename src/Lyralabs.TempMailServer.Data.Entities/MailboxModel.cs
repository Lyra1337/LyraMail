using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lyralabs.TempMailServer.Data.Entities
{
    public class MailboxModel : ModelBase
    {
        [Required]
        public string Address { get; set; }
        
        [Required]
        public string PublicKey { get; set; }

        [Required]
        public string Password { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public List<MailModel> Mails { get; set; }
    }
}
