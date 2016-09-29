using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NFCRing.Service.Core
{
    public class Client
    {
        public TcpClient ClientConnection { get; set; }
        public Thread ClientProcess { get; set; }
    }
}
