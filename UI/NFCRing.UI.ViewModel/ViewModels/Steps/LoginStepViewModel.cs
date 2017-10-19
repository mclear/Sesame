using System;
using System.Security;
using System.Threading.Tasks;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel
{
    public class LoginStepViewModel : BaseStepViewModel
    {
        private readonly IRepositoryService _repositoryService;
        private string _userName;
        private bool _isError;

        public override int Index => 5;

        public override string NextText => "Save";

        public override Func<Task<bool>> NextAction => Save;

        public bool IsError
        {
            get => _isError;
            set => Set(ref _isError, value);
        }

        public string UserName
        {
            get => _userName;
            set => Set(ref _userName, value);
        }

        public SecureString Password { get; set; }

        public LoginStepViewModel(IRepositoryService repositoryService)
        {
            _repositoryService = repositoryService;
        }

        private async Task<bool> Save()
        {
            var password = ConvertToUnsecureString(Password);

            await _repositoryService.SaveAsync();

            return true;
        }

        private string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                return string.Empty;

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}