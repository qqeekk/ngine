using Ngine.Infrastructure.Abstractions.Services;
using System;

namespace Ngine.Infrastructure.Abstractions
{
    public interface IInteractionService
    {
        void OpenFileDialog(IFileFormat format, Action<string> onNext);
        void OpenFolderDialog(Action<string> onNext);
        void SaveFileDialog(IFileFormat format, Action<string> onNext);
        bool AskUserPermission(string message, string title);
        void ShowUserMessage(string message, string title);
        void Navigate<T>(T viewModel, string title) where T : class, IInteractable;
    }
}
