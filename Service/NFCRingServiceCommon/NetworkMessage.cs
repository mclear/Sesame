using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Service.Common
{
    public class NetworkMessage
    {
        public NetworkMessage()
        {

        }

        public NetworkMessage(MessageType t)
        {
            Type = t;
        }

        public MessageType Type { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PluginName { get; set; }
        public string Message { get; set; }
        public string[] Plugins { get; set; }
        public string TokenFriendlyName { get; set; }
    }
}
