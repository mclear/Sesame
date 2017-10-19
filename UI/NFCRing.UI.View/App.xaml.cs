using System;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using NFCRing.UI.ViewModel;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ServiceLocator.Current.GetInstance<MainViewModel>().IsBusy = false;

            var message = e.Exception?.Message;
            if (e.Exception?.InnerException != null)
                message = $"{message}{Environment.NewLine}{e.Exception.InnerException.Message}";

            ServiceLocator.Current.GetInstance<IDialogService>().ShowErrorDialog(message);

            e.Handled = true;
        }
    }
}
