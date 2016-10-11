using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Service.Common
{
    public class PendingRegistration
    {
        public string User { get; set; }
        public string PluginName { get; set; }
        public string Token { get; set; }
        public string Resource { get; set; }
    }
}
