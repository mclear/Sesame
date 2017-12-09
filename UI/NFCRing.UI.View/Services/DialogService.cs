using System.IO;
using System.Windows;
using Microsoft.Win32;
using NFCRing.UI.ViewModel;
using NFCRing.UI.ViewModel.Services;

namespace NFCRing.UI.View.Services
{
    public class DialogService : IDialogService
    {
        private const int MaxImageSizeMb = 4;

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

        public bool ShowImageDialog(out ImageData imageData)
        {
            imageData = new ImageData();
            var ownerWindow = Application.Current.MainWindow;

            var fileDialog = new OpenFileDialog
            {
                Filter = $"Image files (*.bmp, *.jpg, *.jpeg, *.png) | *.bmp; *.jpg; *.jpeg; *.png"
            };

            if (fileDialog.ShowDialog(ownerWindow) != true)
                return false;

            var fileName = fileDialog.FileName;

            if (!File.Exists(fileName))
            {
                ShowErrorDialog("File not found");

                return false;
            }

            var sizeMb = (double)new FileInfo(fileName).Length / 1024 / 1024;
            if (sizeMb > MaxImageSizeMb)
            {
                ShowWarningDialog($"Please choose a smaller file. Max size {MaxImageSizeMb} Mb.");

                return false;
            }

            imageData.ImageBytes = File.ReadAllBytes(fileName);
            imageData.ImageName = Path.GetFileName(fileName);

            return true;
        }
    }
}
