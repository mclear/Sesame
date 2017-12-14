using GalaSoft.MvvmLight;

namespace NFCRing.UI.ViewModel.ViewModels
{
    public class RingItemViewModel : ViewModelBase
    {
        private string _name;
        private string _token;
        private byte[] _image;
        private string _defaultName;

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

        /// <summary>
        /// Gets or sets the Image.
        /// </summary>
        public byte[] Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        public void SetDefaultName(string name)
        {
            _defaultName = name;
        }

        public string GetDefaultName() => _defaultName;
    }
}