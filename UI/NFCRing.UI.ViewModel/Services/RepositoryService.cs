using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFCRing.Service.Common;

namespace NFCRing.UI.ViewModel.Services
{
    public class RepositoryService : IRepositoryService
    {
        public async Task<List<object>> GetRingsAsync()
        {
            await Task.Yield(); // TODO

            return new List<object>();
        }

        public async Task SaveAsync()
        {
            await Task.Delay(1); // TODO
        }
    }

    public interface ITokenService
    {
        Task<Dictionary<string, string>> GetTokensAsync();
    }

    public class TokenService : ITokenService
    {
        public async Task<Dictionary<string, string>> GetTokensAsync()
        {
            TcpClient client = null;

            ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetState) { Username = CurrentUser.Get() }));

            var response = await Task<string>.Factory.StartNew(() =>
            {
                return ServiceCommunication.ReadNetworkMessage(ref client);
            });

            if (string.IsNullOrEmpty(response))
                return null;
            
            UserServerState userServerState = JsonConvert.DeserializeObject<UserServerState>(response);

            await Task.Delay(2000);

            return userServerState.UserConfiguration.Tokens;
        }
    }

    public static class CurrentUser
    {
        public static string Get()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }
    }

    //public interface IClient
    //{
    //    TcpClient Client { get; }
    //}

    //public class CommunicationClient : IClient
    //{
    //    public TcpClient Client { get; }

    //    public CommunicationClient()
    //    {
            
    //    }
    //}
}
