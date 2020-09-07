using System.Windows;

namespace NgineUI.App.Views
{
    internal static class UIHelpers
    {
        public static Window CreateWindow(object view, string title = "Ngine")
        {
            return new Window
            {
                Height = 700,
                Width = 1000,

                Title = title,
                Content = view,
            };
        }
    }
}
