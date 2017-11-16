using System.Windows;
using System.Windows.Controls;
using NFCRing.UI.ViewModel;

namespace NFCRing.UI.View
{
    public abstract class CustomControl : UserControl
    {
        protected CustomControl()
        {
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var initializeViewModel = DataContext as IInitializeAsync;
            if (initializeViewModel != null)
                await initializeViewModel.InitializeAsync();
        }
    }
}