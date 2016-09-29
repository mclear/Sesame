using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.Service.Common
{
    public interface INFCRingServicePlugin
    {
        // runs when the plugin is first loaded
        void PluginLoad();
        // rungs when the plugin is disposed. i.e. on service shutdown
        void PluginUnload();
        // token down - id
        void NCFRingDown(string id, Dictionary<string, object> parameters, SystemState state);
        // token up - id
        void NCFRingUp(string id, Dictionary<string, object> parameters, SystemState state);
        // data read - id, data
        void NFCRingDataRead(string id, byte[] data, Dictionary<string, object> parameters, SystemState state);
        // this should return the name for the plugin so that users can tell what they're registering
        string GetPluginName();
        // returns a list of the parameters and types that we need to provide when activating this plugin
        List<Parameter> GetParameters();
    }

    public class Parameter
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsOptional { get; set; }
        public object Default { get; set; }
    }

    public class SystemState
    {
        public SystemState()
        {
            User = "";
            CredentialData = new Credential();
        }
        public string User { get; set; }
        public SessionState SessionStatus { get; set; }
        public Credential CredentialData { get; set; }
        public bool AwaitingToken { get; set; }
        public TcpClient RegistrationClient { get; set; }
    }

    public enum SessionState
    {
        Active = 1,
        LoggedOff = 2,
        Locked = 3,
    }

    public class Credential
    {
        public Credential()
        {
            ProviderActive = false;
            Client = null;
        }
        public bool ProviderActive { get; set; }
        public TcpClient Client { get; set; }
    }
}
