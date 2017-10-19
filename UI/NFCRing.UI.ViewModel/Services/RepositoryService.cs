using System.Collections.Generic;
using System.Threading.Tasks;

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
}
