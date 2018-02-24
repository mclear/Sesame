using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NFCRing.Service.Common;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public sealed class PlaceRingStepViewModel : BaseStepViewModel
    {
        private readonly ITokenService _tokenService;
        private readonly IDialogService _dialogService;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public override int Index => 2;

        public override bool NextIsVisible => false;

        public override Action CancelAction => Cancel;

        public PlaceRingStepViewModel(ITokenService tokenService, IDialogService dialogService)
        {
            _tokenService = tokenService;
            _dialogService = dialogService;

            ToNext = () => { };
        }

        public override async Task InitializeAsync()
        {

            labelstart:
            var token = await _tokenService.GetNewTokenAsync(_cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(token))
            {
                _dialogService.ShowWarningDialog("Bad token. Please place the another NFC Ring");

                await InitializeAsync();

                return;
            }

            NewRingViewModel.Token = token;

            //here we can integrate the token check if that token already exists

            bool duplicateTag = false;
            try
            {
                string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
                string servicePath = Directory.GetParent(appPath).FullName + @"\Service\Service";
                if (File.Exists(servicePath + @"\Application.config"))
                {
                    string sc = File.ReadAllText(servicePath + @"\Application.config");
                    Config ApplicationConfiguration = JsonConvert.DeserializeObject<Config>(sc);
                    string hashedToken = Crypto.Hash(token);
                    foreach (var item in ApplicationConfiguration.Users)
                    {
                        foreach (var t in item.Tokens)
                        {
                            string dht = Crypto.Hash(hashedToken + item.Salt);
                            if (dht == t.Key)
                            {
                                duplicateTag = true;
                            }
                            else
                            {
                                duplicateTag = false;
                            }
                        }
                    }
                }
                else
                    duplicateTag = false;
            }
            catch (Exception ex)
            {
                
            }
            if (duplicateTag)
            {
                _dialogService.ShowWarningDialog("Duplicate Tag. This tag is already registered");
                goto labelstart;
            }
            else
            {
                ToNext();
            }
        }

        private async void Cancel()
        {
            _cancellationTokenSource.Cancel();

            await _tokenService.SendCancelAsync();
        }
    }
}