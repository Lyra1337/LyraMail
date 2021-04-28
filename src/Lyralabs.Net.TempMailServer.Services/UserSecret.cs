using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lyralabs.Net.TempMailServer
{
    public struct UserSecret
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}
