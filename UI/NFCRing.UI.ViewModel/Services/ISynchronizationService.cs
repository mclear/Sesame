using System;

namespace NFCRing.UI.ViewModel.Services
{
    public interface ISynchronizationService
    {
        void RunInMainThread(Action action);
    }
}