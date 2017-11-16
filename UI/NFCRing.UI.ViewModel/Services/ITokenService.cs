using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NFCRing.UI.ViewModel.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Get tokens, key - hash, value - name.
        /// </summary>
        Task<Dictionary<string, string>> GetTokensAsync(string userName);

        Task RemoveTokenAsync(string token);
        Task AddTokenAsync(string userName, string password, string token);
        Task<string> GetNewTokenAsync(CancellationToken cancellationToken);
    }
}