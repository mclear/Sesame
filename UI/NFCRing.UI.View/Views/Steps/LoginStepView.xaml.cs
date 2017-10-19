using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NFCRing.UI.ViewModel;

namespace NFCRing.UI.View.Views
{
    /// <summary>
    /// Interaction logic for LoginStepView.xaml
    /// </summary>
    public partial class LoginStepView
    {
        public LoginStepView()
        {
            InitializeComponent();
        }

        #region Password

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is PasswordBox passwordBox))
                return;

            PasswordTextBox.Text = passwordBox.Password;
            if (!(DataContext is LoginStepViewModel viewModel))
                return;

            viewModel.Password = passwordBox?.SecurePassword;
        }

        private void ShowPasswordButton_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        #endregion
    }


}
