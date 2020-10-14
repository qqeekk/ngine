using Microsoft.Win32;
using Ngine.Infrastructure.Abstractions;
using Ngine.Infrastructure.Abstractions.Services;
using ReactiveUI;
using System;
using System.Windows;

namespace NgineUI.App.Views
{
    internal class UIHelpers : IInteractionService
    {
        private static string CreateFileFilter(IFileFormat format) =>
            $"{format.FileFormatDescription} (*.{format.FileExtension})|*.{format.FileExtension}";

        private static Window CreateWindow(object view, string title = "Ngine")
        {
            return new Window
            {
                Height = 700,
                Width = 1000,

                Title = title,
                Content = view,
            };
        }

        public void Navigate<T>(T viewModel, string title) where T : class, IInteractable
        {
            var view = Splat.Locator.Current.GetService(typeof(IViewFor<T>));
            ((IViewFor)view).ViewModel = viewModel;

            var window = CreateWindow(view, title);
            viewModel.FinishInteraction = window.Close;
            window.ShowDialog();
            viewModel.FinishInteraction = () => { };
        }

        public void OpenFileDialog(IFileFormat format, Action<string> onNext)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = CreateFileFilter(format),
            };

            if (fileDialog.ShowDialog() == true)
            {
                onNext(fileDialog.FileName);
            }
        }

        public void OpenFolderDialog(Action<string> onNext)
        {
            using var fileDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                onNext(fileDialog.SelectedPath);
            }
        }

        public void SaveFileDialog(IFileFormat format, Action<string> onNext)
        {
            var fileDialog = new SaveFileDialog
            {
                Filter = CreateFileFilter(format),
            };

            if (fileDialog.ShowDialog() == true)
            {
                onNext(fileDialog.FileName);
            }
        }

        public bool AskUserPermission(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel);
            return result switch
            {
                MessageBoxResult.OK => true,
                _ => false
            };
        }

        public void ShowUserMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
