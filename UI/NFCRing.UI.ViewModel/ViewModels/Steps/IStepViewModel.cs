using System;
using System.Threading.Tasks;

namespace NFCRing.UI.ViewModel
{
    public interface IStepViewModel
    {
        int Index { get; }
        bool CancelIsVisible { get; }
        string NextText { get; }
        Func<Task<bool>> NextAction { get; }
    }
}
