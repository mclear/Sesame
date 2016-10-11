using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Service.Common
{
    public enum ServiceState
    {
        Starting,
        Running,
        Stopping,
        Stopped
    }
    public enum MessageType
    {
        GetToken,
        RegisterToken,
        Token,
        AssociatePluginToToken,
        CancelRegistration,
        GetUserCredential,
        UserCredential,
        GetState,
        State,
        Message
    }
}
