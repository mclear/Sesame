using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private string _VersionInfo;
        public string VersionInfo
        {
            get
            {
                return _VersionInfo;
            }
            set
            {
                _VersionInfo = value;
                RaisePropertyChanged();
            }
        }
        public AboutViewModel()
        {
        }
    }
}
