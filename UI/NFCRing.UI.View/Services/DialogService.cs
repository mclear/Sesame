using System.Windows;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.View.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowQuestionDialog(string message)
        {
            return ShowMessageDialog(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public bool ShowErrorDialog(string message)
        {
            return ShowMessageDialog(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowWarningDialog(string message)
        {
            return ShowMessageDialog(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool ShowMessageDialog(string message, string caption, MessageBoxButton buttons, MessageBoxImage image)
        {
            var dialogResult = MessageBox.Show(Application.Current.MainWindow, message, caption, buttons, image);

            return dialogResult == MessageBoxResult.OK || dialogResult == MessageBoxResult.Yes;
        }
    }
}
