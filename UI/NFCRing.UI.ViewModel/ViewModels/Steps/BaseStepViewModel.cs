using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace NFCRing.UI.ViewModel
{
    public abstract class BaseStepViewModel : ViewModelBase, IStepViewModel
    {
        public abstract int Index { get; }
        public virtual bool CancelIsVisible => true;
        public virtual string NextText => "Next";
        public virtual Func<Task<bool>> NextAction => null;
    }
}