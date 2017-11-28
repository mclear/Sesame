using GalaSoft.MvvmLight;

namespace NFCRing.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ContentViewModel _content;
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(ref _isBusy, value); }
        }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        public ContentViewModel Content
        {
            get { return _content; }
            set { Set(ref _content, value); }
        }

        public void SetContent(ContentViewModel content)
        {
            Content = content;
        }
    }
}
