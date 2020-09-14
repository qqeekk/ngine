using Ngine.Infrastructure.Abstractions.Services;
using ReactiveUI;
using System.Reactive;

namespace NgineUI.ViewModels.Control
{
    public class HeaderViewModel: ReactiveObject
    {
        public HeaderViewModel(IFileFormat schemaFileFormat)
        {
            SchemaFileFormat = schemaFileFormat.FileExtension;
        }

        public ReactiveCommand<Unit, Unit> SaveModelCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> SaveAsModelCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> SaveKerasModelCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> ReadModelCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> ConfigureTrainingCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> ConfigureTuningCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> RunTraingCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> RunTuningCommand { get; internal set; }

        public string SchemaFileFormat { get; }
    }
}
