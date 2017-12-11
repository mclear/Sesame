using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace NFCRing.UI.View
{
    public class EditSelectionBehavior : Behavior<Image>
    {
        public static readonly DependencyProperty TextBoxProperty = DependencyProperty.Register(
            "TextBox", typeof(TextBox), typeof(EditSelectionBehavior), new PropertyMetadata(default(TextBox)));

        public TextBox TextBox
        {
            get { return (TextBox) GetValue(TextBoxProperty); }
            set { SetValue(TextBoxProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseUp += AssociatedObjectOnMouseUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseUp -= AssociatedObjectOnMouseUp;

            base.OnDetaching();
        }

        private void AssociatedObjectOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TextBox == null)
                return;

            TextBox.Focus();
            TextBox.SelectAll();
        }
    }
}