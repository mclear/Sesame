using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFCRing.Service.Common;

namespace NFCRing.UI.ViewModel.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUserCredentials _userCredentials;
        private readonly ILogger _logger;

        public TokenService(IUserCredentials userCredentials, ILogger logger)
        {
            _userCredentials = userCredentials;
            _logger = logger;
        }

        public bool Ping()
        {
            TcpClient client = null;

            var result = ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.State)));

            if (result == 0)
            {
                _logger.Error($"Service not available");

                return false;
            }

            return true;
        }

        public async Task<Dictionary<string, string>> GetTokensAsync(string userName)
        {
            TcpClient client = null;

            ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetState) { Username = userName }));

            var response = await Task<string>.Factory.StartNew(() =>
            {
                return ServiceCommunication.ReadNetworkMessage(ref client);
            });

            if (string.IsNullOrEmpty(response))
                return new Dictionary<string, string>();
            
            UserServerState userServerState = JsonConvert.DeserializeObject<UserServerState>(response);

            _logger.Trace($"GetTokensAsync: {JsonConvert.SerializeObject(userServerState.UserConfiguration.Tokens)}");

            return userServerState.UserConfiguration.Tokens ?? new Dictionary<string, string>();
        }

        public async Task RemoveTokenAsync(string token)
        {
            TcpClient client = null;

            ServiceCommunication.SendNetworkMessage(ref client,
                JsonConvert.SerializeObject(new NetworkMessage(MessageType.Delete) {Token = token, Username = _userCredentials.GetName()}));

            _logger.Trace($"RemoveTokenAsync: {token}");

            await Task.Yield();
        }

        public async Task AddTokenAsync(string userName, string password, string token)
        {
            TcpClient client = null;

            var ringName = await GetRingNameAsync(userName);

            await Task.Factory.StartNew(() =>
            {
                ServiceCommunication.SendNetworkMessage(ref client,
                    JsonConvert.SerializeObject(new NetworkMessage(MessageType.RegisterToken)
                    {
                        TokenFriendlyName = ringName,
                        Username = userName,
                        Password = password,
                        Token = token
                    }));
            });

            _logger.Trace($"AddTokenAsync: username: {userName} token: {token}");
        }

        public async Task<string> GetNewTokenAsync(CancellationToken cancellationToken)
        {
            var newToken = await Task.Factory.StartNew(() =>
            {
                return GetNewToken(cancellationToken);
            }, cancellationToken);

            _logger.Trace($"GetNewTokenAsync: {newToken}");

            return newToken;
        }

        public async Task SendCancelAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                TcpClient client = null;

                SendCancel(ref client);
            });
        }

        private string GetNewToken(CancellationToken cancellationToken)
        {
            TcpClient client = null;

            _logger.Trace($"Send GetToken");

            var getTokenMessage = ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetToken)));

            _logger.Trace($"GetToken: {getTokenMessage}");

            if (getTokenMessage <= 0)
                return null;

            _logger.Trace($"Send ReadNetworkMessage");

            var i = 0;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var message = ServiceCommunication.ReadNetworkMessage(ref client);
                if (!string.IsNullOrEmpty(message))
                {
                    _logger.Trace($"GetNewToken: {message}");
                    var networkMessage = JsonConvert.DeserializeObject<NetworkMessage>(message);
                    return networkMessage?.Token;
                }

#if DEBUG
                i++;

                if (i > 10)
                {
                    ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.CancelRegistration)));

                    return "23442453456";
                }

                Thread.Sleep(200);
#endif

                Thread.Sleep(50);
            }

            SendCancel(ref client);

            return null;
        }

        private void SendCancel(ref TcpClient client)
        {
            _logger.Trace("Send CancelRegistration");

            var cancelMessage = ServiceCommunication.SendNetworkMessage(ref client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.CancelRegistration)));

            _logger.Trace($"CancelRegistration: {cancelMessage}");
        }

        private async Task<string> GetRingNameAsync(string login)
        {
            var oldTokens = await GetTokensAsync(login);

            var id = "00";

            for (var i = 1; i < _userCredentials.MaxTokensCount; i++)
            {
                var name = i.ToString("00");

                if (!oldTokens.Values.Any(x => x.Contains(name)))
                {
                    id = name;
                    break;
                }
            }

            return $"ring {id}";
        }
    }
}