using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Lyralabs.TempMailServer.Data
{
    public class UserModel : IdentityUser<int>
    {
        public string Username { get; set; }
        public string MailboxKeys { get; set; }
        public string EncryptedSymmetricKey { get; set; }
        public string EncryptedPrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}
