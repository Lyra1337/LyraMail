using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lyralabs.TempMailServer.Web
{
    public class UserState
    {
        public string CurrentMailbox { get; set; }
        public UserSecret? Secret { get; set; }
    }
}
