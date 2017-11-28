using System;
using System.Windows;
using System.Windows.Threading;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.View.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        public void RunInMainThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Normal);
        }
    }
}
