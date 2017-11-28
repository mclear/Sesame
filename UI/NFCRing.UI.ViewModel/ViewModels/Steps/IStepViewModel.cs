using System;
using System.Threading.Tasks;

namespace NFCRing.UI.ViewModel
{
    public interface IStepViewModel
    {
        int Index { get; }

        bool CancelIsVisible { get; }
        bool NextIsVisible { get; }

        string NextText { get; }

        /// <summary>
        /// Return use default wizard behavior for next button.
        /// </summary>
        Func<Task<bool>> NextAction { get; }

        Action CancelAction { get; }

        Action ToNext { get; set; }

        NewRingViewModel NewRingViewModel { get; set; }
    }
}
