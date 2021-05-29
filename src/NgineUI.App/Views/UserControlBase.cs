using ReactiveUI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NgineUI.App.Views.Network
{
    public abstract class UserControlBase: UserControl
    {
        protected virtual void EnterErrorState(System.Windows.Controls.Control control, object errorObject)
        {
            control.Foreground = (Brush)(Resources["ErrorForegroundColor"] ??= Brushes.Red);
            control.ToolTip = errorObject;
        }

        protected virtual void ExitErrorState(System.Windows.Controls.Control control)
        {
            control.Foreground = (Brush)(Resources["DefaultForegroundColor"] ??= Brushes.Black);
            control.ToolTip = null;
        }
    }
}
