using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Service.Common
{
    public class UserServerState
    {
        public User UserConfiguration { get; set; }
        public List<PluginInfo> Plugins { get; set; }
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}
