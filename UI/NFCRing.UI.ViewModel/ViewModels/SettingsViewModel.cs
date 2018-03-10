using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {

        const string CONFGIPATH = "C:\\Windows\\nfcfencesettings.config";
        public SettingsViewModel()
        {
            SaveCommand = new RelayCommand(SaveCommandMethod);
            SetValues();
        }

        private void SetValues()
        {
            if (File.Exists(CONFGIPATH))
            {
                try
                {
                    string values = File.ReadAllText(CONFGIPATH);
                    if (values == "y")
                        UseDefaultLogin = true;
                    else
                        UseDefaultLogin = false;
                }
                catch (Exception ex)
                {
                    UseDefaultLogin = false;
                }
            }
            else
                UseDefaultLogin = false;
        }

        private void SaveCommandMethod()
        {
            //save it from here
            string dataToWrite = string.Empty;
            if (UseDefaultLogin == true)
            {
                dataToWrite = "y";
            }
            else
            {
                dataToWrite = "n";
            }
            File.WriteAllText(CONFGIPATH, dataToWrite);
            CloseSignal = true;
            MessengerInstance.Send<SettingsViewModel>(this);
        }

        private bool useDefaultLogin = false;

        public bool UseDefaultLogin
        {
            get { return useDefaultLogin; }
            set { useDefaultLogin = value; RaisePropertyChanged(); }
        }

        public RelayCommand SaveCommand { get; }
        public bool CloseSignal { get; set; }
    }
}
