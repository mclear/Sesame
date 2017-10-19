using System;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace NFCRing.UI.ViewModel
{
    public class FinishedStepViewModel : BaseStepViewModel
    {
        public override int Index => 6;
        public override bool CancelIsVisible => false;
        public override string NextText => "Finish";
        public override Func<Task<bool>> NextAction => Finish;

        private async Task<bool> Finish()
        {
            await Task.Yield();

            ServiceLocator.Current.GetInstance<MainViewModel>()
                .SetContent(ServiceLocator.Current.GetInstance<LoginControlViewModel>());

            return false;
        }
    }
}