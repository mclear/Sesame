using System.Collections.Generic;
using System.Threading.Tasks;

namespace NFCRing.UI.ViewModel.Services
{
    public interface IRepositoryService
    {
        Task<List<object>> GetRingsAsync();
        Task SaveAsync();
    }
}
