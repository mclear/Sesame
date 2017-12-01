using System;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public class HelloStepViewModel : BaseStepViewModel
    {
        public override int Index => 1;

        public RelayCommand NavigateCommand { get; }

        public HelloStepViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);
        }

        private void Navigate()
        {
            try
            {
                System.Diagnostics.Process.Start("http://nfcring.com/privacy");
            }
            catch (Exception exception)
            {
                ServiceLocator.Current.GetInstance<IDialogService>().ShowErrorDialog(exception.Message);
            }
        }
    }
}