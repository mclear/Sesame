using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFCRing.UI.ViewModel.ViewModels;

namespace NFCRing.UI.View.Controls
{
    /// <summary>
    /// Interaction logic for SkypeTextEditControl.xaml
    /// </summary>
    public partial class SkypeTextEditControl
    {
        private static FrameworkElement _endEditFrameworkElement;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(SkypeTextEditControl), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty OkCommandProperty = DependencyProperty.Register(
            "OkCommand", typeof(RelayCommand<object>), typeof(SkypeTextEditControl), new PropertyMetadata(default(RelayCommand<object>)));

        public RelayCommand<object> OkCommand
        {
            get { return (RelayCommand<object>) GetValue(OkCommandProperty); }
            set { SetValue(OkCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
            "CancelCommand", typeof(RelayCallbackCommand<object>), typeof(SkypeTextEditControl), new PropertyMetadata(default(RelayCommand<object>), CancelCommandPropertyChangedCallback));

        private static void CancelCommandPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var command = e.NewValue as RelayCallbackCommand<object>;
            if (command == null)
                return;

            command.Callback = () =>
            {
                if (!_endEditFrameworkElement.IsFocused)
                    _endEditFrameworkElement.Focus();
            };
        }

        public RelayCallbackCommand<object> CancelCommand
        {
            get { return (RelayCallbackCommand<object>) GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(SkypeTextEditControl), new PropertyMetadata(default(object)));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public SkypeTextEditControl()
        {
            InitializeComponent();
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            // TODO: workaround for lost focus

            _endEditFrameworkElement = sender as FrameworkElement;
        }

        private void NameTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                RemoveFocus();
            }
            else if (e.Key == Key.Enter)
            {
                if (OkCommand?.CanExecute(CommandParameter) == true)
                {
                    OkCommand.Execute(CommandParameter);
                    RemoveFocus();
                }
            }
        }

        private void NameTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            CancelCommand?.Execute(CommandParameter);
        }

        private void CommandButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RemoveFocus();
        }

        private void RemoveFocus()
        {
            if (!_endEditFrameworkElement.IsFocused)
                _endEditFrameworkElement?.Focus();
        }
    }
}
