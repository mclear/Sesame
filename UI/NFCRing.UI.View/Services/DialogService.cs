using System.Windows;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.View.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowQuestionDialog(string questionMessage)
        {
            return ShowMessageDialog(questionMessage, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public bool ShowErrorDialog(string errorMessage)
        {
            return ShowMessageDialog(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool ShowMessageDialog(string questionMessage, string caption, MessageBoxButton buttons, MessageBoxImage image)
        {
            var dialogResult = MessageBox.Show(Application.Current.MainWindow, questionMessage, caption, buttons, image);

            return dialogResult == MessageBoxResult.OK || dialogResult == MessageBoxResult.Yes;
        }
    }
}
