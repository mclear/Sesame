using System;
using System.Windows;
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

            ServiceLocator.Current.GetInstance<ILogger>().Error(message);
            ServiceLocator.Current.GetInstance<IDialogService>().ShowErrorDialog(message);

            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceLocator.Current.GetInstance<ILogger>().Info("NFC Ring startup");

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceLocator.Current.GetInstance<ILogger>().Info("NFC Ring exit");

            base.OnExit(e);
        }
    }
}
