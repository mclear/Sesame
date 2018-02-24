using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public class LoginControlViewModel : ContentViewModel, IInitializeAsync
    {
        private readonly IDialogService _dialogService;
        private readonly ITokenService _tokenService;
        private readonly ISynchronizationService _synchronizationService;
        private readonly IUserCredentials _userCredentials;
        private readonly ILogger _logger;
        private ObservableCollection<RingItemViewModel> _items;
        private RingItemViewModel _selectedItem;
        private bool _isBusy;
        private bool _serviceStarted;
        private ObservableCollection<NFCDevice> devicesList;
        private bool _IsDeviceNotAvailable = false;
        private bool _IsDeviceAvailable = false;

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
        public ObservableCollection<NFCDevice> DevicesList
        {
            get { return devicesList ?? (devicesList = new ObservableCollection<NFCDevice>()); }
            set
            {
                Set(ref devicesList, value);
                if (devicesList != null)
                {
                    if (devicesList.Count > 0)
                    {
                        IsDeviceAvailable = true;
                        IsDeviceNotAvailable = false;
                    }
                    else
                    {
                        IsDeviceAvailable = false;
                        IsDeviceNotAvailable = true;
                    }
                }
            }
        }


        public bool IsDeviceNotAvailable
        {
            get { return _IsDeviceNotAvailable; }
            set
            {
                _IsDeviceNotAvailable = value;
                RaisePropertyChanged();
            }
        }
        public bool IsDeviceAvailable
        {
            get { return _IsDeviceAvailable; }
            set
            {
                _IsDeviceAvailable = value;
                RaisePropertyChanged();
            }
        }

        public bool AllowAdd => _serviceStarted && Items.Count < _userCredentials.MaxTokensCount;

        /// <summary>
        /// Add new ring item command.
        /// </summary>
        public RelayCommand AddCommand { get; }

        /// <summary>
        /// About command.
        /// </summary>
        public RelayCommand AboutCommand { get; }

        /// <summary>
        /// Remove ring item command.
        /// </summary>
        public RelayCommand<RingItemViewModel> RemoveCommand { get; }

        /// <summary>
        /// Select image command.
        /// </summary>
        public RelayCommand<string> SelectImageCommand { get; }

        /// <summary>
        /// Save name command.
        /// </summary>
        public RelayCommand<object> SaveNameCommand { get; }

        /// <summary>
        /// Cancel edit name command.
        /// </summary>
        public RelayCallbackCommand<object> CancelEditNameCommand { get; }

        /// <summary>
        /// Refresh Connected NFC Devices command.
        /// </summary>
        public RelayCallbackCommand<object> RefreshConnectedDevicesCommand { get; }

        /// <summary>
        /// Ctor.
        /// </summary>
        public LoginControlViewModel(IDialogService dialogService, ITokenService tokenService,
            ISynchronizationService synchronizationService, IUserCredentials userCredentials, ILogger logger)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((x) =>
            {
                try
                {
                    DevicesList = NFCWMQService.GetConnectedDevices();
                }
                catch (Exception)
                {

                }
            });
            _dialogService = dialogService;
            _tokenService = tokenService;
            _synchronizationService = synchronizationService;
            _userCredentials = userCredentials;
            _logger = logger;

            Title = "NFCRing - Fence";

            AddCommand = new RelayCommand(Add, () => AllowAdd);
            RemoveCommand = new RelayCommand<RingItemViewModel>(Remove);
            SelectImageCommand = new RelayCommand<string>(SelectImage);
            SaveNameCommand = new RelayCommand<object>(SaveName, x => !string.IsNullOrEmpty(Items.FirstOrDefault(y => Equals(x, y.Token))?.Name));
            CancelEditNameCommand = new RelayCallbackCommand<object>(CancelEditName);
            RefreshConnectedDevicesCommand = new RelayCallbackCommand<object>(RefreshConnectedDevices);
            AboutCommand=new RelayCommand(AboutCommandMethod);
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

            var tokens = await _tokenService.GetTokensAsync(_userCredentials.GetName());

            if (Items.Any())
                _synchronizationService.RunInMainThread(() => Items.Clear());

            foreach (var token in tokens)
            {
                var tokenKey = token.Key;
                var imageData = _tokenService.GetTokenImage(tokenKey);
                var ringItemViewModel =
                    new RingItemViewModel {Name = token.Value, Token = tokenKey, Image = imageData.ImageBytes};
                ringItemViewModel.SetDefaultName(ringItemViewModel.Name);

                _synchronizationService.RunInMainThread(() => Items.Add(ringItemViewModel));
            }
           
            _synchronizationService.RunInMainThread(() =>
            {
                RaisePropertyChanged(nameof(AllowAdd));
                AddCommand.RaiseCanExecuteChanged();
            });
        }

        private void Add()
        {
            ServiceLocator.Current.GetInstance<MainViewModel>()
                .SetContent(ServiceLocator.Current.GetInstance<WizardViewModel>());
        }
        private void AboutCommandMethod()
        {
            string version = new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductVersion).ToString();
            Messenger.Default.Send(new AboutViewModel() { VersionInfo= version });
        }

        private async void Remove(RingItemViewModel item)
        {
            if (item == null)
                return;

            if (!_dialogService.ShowQuestionDialog($"Remove {item.Name}?"))
                return;

            await RemoveAsync(item.Token);
        }

        private async void SaveName(object token)
        {
            var item = Items.FirstOrDefault(x => Equals(x.Token, token));
            if (item == null || item.Name == item.GetDefaultName())
                return;

            item.SetDefaultName(item.Name);
            await _tokenService.UpdateNameAsync(item.Token, item.Name);
        }

        private void CancelEditName(object token)
        {
            var item = Items.FirstOrDefault(x => Equals(x.Token, token));
            if (item == null)
                return;

            item.Name = item.GetDefaultName();
            CancelEditNameCommand.Callback?.Invoke();
        }
        private async void RefreshConnectedDevices(object state)
        {
            try
            {
                DevicesList = await Task.Factory.StartNew(() => NFCWMQService.GetConnectedDevices());
            }
            catch (Exception ex)
            {
                
            }
        }
        private async Task RemoveAsync(string token)
        {
            await _tokenService.RemoveTokenAsync(token);

            // TODO: workaround
            await Task.Delay(50);

            await InitializeAsync();
        }

        private void SelectImage(string token)
        {
            var item = Items.FirstOrDefault(x => x.Token == token);
            if (item == null)
                return;

            ImageData imageData;
            if (!_dialogService.ShowImageDialog(out imageData))
                return;

            try
            {
                _tokenService.UpdateTokenImage(token, imageData);
            }
            catch (Exception ex)
            {
                var message = $"Error image saving: {ex.Message}";
                _logger.Error($"{message}{Environment.NewLine}{ex}");
                _dialogService.ShowErrorDialog(message);
                return;
            }

            item.Image = imageData.ImageBytes;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsBusy))
                ServiceLocator.Current.GetInstance<MainViewModel>().IsBusy = IsBusy;
        }
    }

    public class RelayCallbackCommand<T> : RelayCommand<T>
    {
        public Action Callback { get; set; }

        public RelayCallbackCommand(Action<T> execute) : base(execute)
        {
        }

        public RelayCallbackCommand(Action<T> execute, Func<T, bool> canExecute) : base(execute, canExecute)
        {
        }
    }
}
