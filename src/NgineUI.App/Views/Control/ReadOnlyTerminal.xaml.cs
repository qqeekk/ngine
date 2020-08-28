using Ngine.Infrastructure.Services.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NgineUI.App.Views.Control
{
    /// <summary>
    /// Interaction logic for ReadOnlyTerminal.xaml
    /// </summary>
    public partial class ReadOnlyTerminal : UserControl
    {
        private readonly ConsoleRedirectWriter consoleRedirectWriter = new ConsoleRedirectWriter();
        //string LastConsoleString;

        public ReadOnlyTerminal()
        {
            InitializeComponent();
            Application.Current.MainWindow.Closed += (o, e) => consoleRedirectWriter.Release(); //sets releases console when window closes.
        }

        private void tbxTerminal_Initialized(object sender, EventArgs e)
        {
            // Use this for thread safe objects or UIElements in a single thread program
            // consoleRedirectWriter.OnWrite += delegate (string value) { LastConsoleString = value; };

            // Multithread operation - Use the dispatcher to write to WPF UIElements if there is more than 1 thread.
            consoleRedirectWriter.OnWrite += text =>
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action<string>)AppendText, text);
        }

        private void AppendText(string value)
        {
            tbxTerminal.AppendText(value);
            tbxTerminal.ScrollToEnd();
        }
    }
}