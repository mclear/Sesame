using NFCRing.Service.Common;
using System.DirectoryServices.AccountManagement;

namespace NFCRing.UI.ViewModel.Services
{
    public class UserCredentials : IUserCredentials
    {
        public int MaxTokensCount => 100;

        public string GetName()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        public bool IsValidCredentials(string username, string password)
        {
            var contextType = Crypto.IsDomainJoined() // check if we're on a domain
                ? ContextType.Domain
                : ContextType.Machine;


            using (PrincipalContext context = new PrincipalContext(contextType))
            {
                return context.ValidateCredentials(username, password);
            }
        }
    }
}
