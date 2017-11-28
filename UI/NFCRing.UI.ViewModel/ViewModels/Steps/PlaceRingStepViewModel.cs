using System;
using System.Threading;
using System.Threading.Tasks;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel
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

            ToNext = () => {};
        }

        public override async Task InitializeAsync()
        {
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

            ToNext();
        }

        private async void Cancel()
        {
            _cancellationTokenSource.Cancel();

            await _tokenService.SendCancelAsync();
        }
    }
}