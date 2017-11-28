using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel
{
    public class LoginControlViewModel : ContentViewModel, IInitializeAsync
    {
        private readonly IDialogService _dialogService;
        private readonly ITokenService _tokenService;
        private readonly ISynchronizationService _synchronizationService;
        private readonly ILogger _logger;
        private ObservableCollection<RingItemViewModel> _items;
        private RingItemViewModel _selectedItem;
        private bool _isBusy;
        private bool _serviceStarted;

        public ObservableCollection<RingItemViewModel> Items
        {
            get { return _items ?? (_items = new ObservableCollection<RingItemViewModel>()); }
            set { Set(ref _items, value); }
        }

        public RingItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        public bool AllowAdd => _serviceStarted && Items.Count < CurrentUser.MaxTokensCount;

        /// <summary>
        /// Add new ring item command.
        /// </summary>
        public RelayCommand AddCommand { get; }

        /// <summary>
        /// Remove ring item command.
        /// </summary>
        public RelayCommand<RingItemViewModel> RemoveCommand { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        public LoginControlViewModel(IDialogService dialogService, ITokenService tokenService, ISynchronizationService synchronizationService, ILogger logger)
        {
            _dialogService = dialogService;
            _tokenService = tokenService;
            _synchronizationService = synchronizationService;
            _logger = logger;

            Title = "NFC Ring Login Control";

            AddCommand = new RelayCommand(Add, () => AllowAdd);
            RemoveCommand = new RelayCommand<RingItemViewModel>(Remove);

            PropertyChanged += OnPropertyChanged;
        }

        public async Task InitializeAsync()
        {
            _serviceStarted = await Task.Factory.StartNew(() => _tokenService.Ping());

            if (!_serviceStarted)
            {
                _synchronizationService.RunInMainThread(() => RaisePropertyChanged(nameof(AllowAdd)));

                _dialogService.ShowErrorDialog("Service not available");

                return;
            }

            var tokens = await _tokenService.GetTokensAsync(CurrentUser.Get());

            if (Items.Any())
                _synchronizationService.RunInMainThread(() => Items.Clear());

            foreach (var token in tokens)
            {
                _synchronizationService.RunInMainThread(() => Items.Add(new RingItemViewModel { Name = token.Value, Token = token.Key }));
            }

            _synchronizationService.RunInMainThread(() =>
            {
                RaisePropertyChanged(nameof(AllowAdd));
                AddCommand.RaiseCanExecuteChanged();
            });
        }

        private void Add()
        {
            ServiceLocator.Current.GetInstance<MainViewModel>().SetContent(ServiceLocator.Current.GetInstance<WizardViewModel>());
        }

        private async void Remove(RingItemViewModel item)
        {
            if (item == null)
                return;

            if (!_dialogService.ShowQuestionDialog($"Remove {item.Name}"))
                return;

            await RemoveAsync(item.Token);
        }

        private async Task RemoveAsync(string token)
        {
            await _tokenService.RemoveTokenAsync(token);

            // TODO: workaround
            await Task.Delay(50);

            await InitializeAsync();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsBusy))
                ServiceLocator.Current.GetInstance<MainViewModel>().IsBusy = IsBusy;
        }
    }
}
