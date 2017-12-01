using System;
using System.Threading;
using System.Threading.Tasks;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public sealed class RemoveRingStepViewModel : BaseStepViewModel
    {
        private readonly ITokenService _tokenService;
        private readonly IDialogService _dialogService;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public override int Index => 4;

        public override bool NextIsVisible => false;

        public override Action CancelAction => Cancel;

        public RemoveRingStepViewModel(ITokenService tokenService, IDialogService dialogService)
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

            var isConfirmed = !string.IsNullOrEmpty(token) && Equals(NewRingViewModel.Token, token);

            if (!isConfirmed)
            {
                _dialogService.ShowWarningDialog("Bad confirmation token. Please place the previous NFC Ring");

                await InitializeAsync();

                return;
            }

            await _tokenService.AddTokenAsync(NewRingViewModel.Login, NewRingViewModel.Password, NewRingViewModel.Token);

            ToNext();
        }

        private async void Cancel()
        {
            _cancellationTokenSource.Cancel();

            await _tokenService.SendCancelAsync();
        }
    }
}