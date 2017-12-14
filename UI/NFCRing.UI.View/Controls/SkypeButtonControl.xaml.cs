using System.Windows;
using System.Windows.Media;

namespace NFCRing.UI.View.Controls
{
    /// <summary>
    /// Interaction logic for SkypeButtonControl.xaml
    /// </summary>
    public partial class SkypeButtonControl
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof(ImageSource), typeof(SkypeButtonControl), new PropertyMetadata(default(ImageSource)));

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public SkypeButtonControl()
        {
            InitializeComponent();
        }
    }
}
