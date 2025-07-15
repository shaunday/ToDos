using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Todos.Ui.Resources
{
    public static class NoSelectOnClickBehavior
    {
        public static readonly DependencyProperty PreventRowSelectProperty =
            DependencyProperty.RegisterAttached(
                "PreventRowSelect",
                typeof(bool),
                typeof(NoSelectOnClickBehavior),
                new PropertyMetadata(false, OnPreventRowSelectChanged));

        public static bool GetPreventRowSelect(Button button) => (bool)button.GetValue(PreventRowSelectProperty);
        public static void SetPreventRowSelect(Button button, bool value) => button.SetValue(PreventRowSelectProperty, value);

        private static void OnPreventRowSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                if ((bool)e.NewValue)
                    button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
                else
                    button.PreviewMouseLeftButtonDown -= Button_PreviewMouseLeftButtonDown;
            }
        }

        private static void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent DataGrid row selection, but allow the button to process the click and execute its command
            e.Handled = true;

            // Manually raise the command
            if (sender is Button button && button.Command != null)
            {
                if (button.Command.CanExecute(button.CommandParameter))
                    button.Command.Execute(button.CommandParameter);
            }
        }
    }
} 