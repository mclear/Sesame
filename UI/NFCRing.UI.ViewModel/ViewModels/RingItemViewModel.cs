using GalaSoft.MvvmLight;

namespace NFCRing.UI.ViewModel
{
    public class RingItemViewModel : ViewModelBase
    {
        private string _name;
        private string _token;

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        /// <summary>
        /// Gets or sets the Token.
        /// </summary>
        public string Token
        {
            get { return _token; }
            set { Set(ref _token, value); }
        }
    }
}