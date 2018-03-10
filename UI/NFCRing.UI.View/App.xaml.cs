using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using NFCRing.UI.ViewModel.ViewModels;
using NFCRing.UI.ViewModel.Services;
using GalaSoft.MvvmLight.Messaging;
using NFCRing.UI.View.Views;

namespace NFCRing.UI.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        SettingsWindow settingsWindow = null;
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceLocator.Current.GetInstance<ILogger>().Info("NFC Ring startup");
            Messenger.Default.Register<AboutViewModel>(this, ProcessAboutMessage);
            Messenger.Default.Register<SettingsViewModel>(this, ProcessSettingsMessage);
            base.OnStartup(e);
        }
        private void ProcessAboutMessage(AboutViewModel message)
        {
            AboutWindow about = new AboutWindow();
            about.DataContext = message;
            about.ShowDialog();
        }
        private void ProcessSettingsMessage(SettingsViewModel message)
        {
            if (message != null && message.CloseSignal == true)
            {
                settingsWindow.Close();
            }
            else
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.DataContext = message;
                settingsWindow.ShowDialog();
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            ServiceLocator.Current.GetInstance<ILogger>().Info("NFC Ring exit");

            base.OnExit(e);
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
    }
}
