using System;
using System.Security;
using System.Threading.Tasks;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel
{
    public class LoginStepViewModel : BaseStepViewModel
    {
        private readonly ITokenService _tokenService;
        private readonly IDialogService _dialogService;
        private string _userName;
        private bool _isError;

        public override int Index => 5;

        public override string NextText => "Save";

        public override Func<Task<bool>> NextAction => Save;

        public bool IsError
        {
            get { return _isError; }
            set { Set(ref _isError, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }

        public SecureString Password { get; set; }

        public LoginStepViewModel(ITokenService tokenService, IDialogService dialogService)
        {
            _tokenService = tokenService;
            _dialogService = dialogService;

            UserName = CurrentUser.Get();
        }

        private async Task<bool> Save()
        {
            var password = ConvertToUnsecureString(Password);

            NewRingViewModel.Login = UserName;
            NewRingViewModel.Password = password;

            if (!Validate())
                return false;

            await _tokenService.AddTokenAsync(NewRingViewModel.Login, NewRingViewModel.Password, NewRingViewModel.Token);

            return true;
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(NewRingViewModel.Login))
            {
                _dialogService.ShowErrorDialog("Please input User name");

                return false;
            }

            if (string.IsNullOrEmpty(NewRingViewModel.Password))
            {
                _dialogService.ShowErrorDialog("Please input password");

                return false;
            }

            if (!CurrentUser.IsValidCredentials(NewRingViewModel.Login, NewRingViewModel.Password))
            {
                _dialogService.ShowErrorDialog("Please input valid User credentials");

                return false;
            }

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