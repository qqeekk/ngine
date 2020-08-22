using ReactiveUI;
using System.Reactive;

namespace NgineUI.ViewModels.Control
{
    public class HeaderViewModel: ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> SaveModelCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> ReadModelCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> ConfigureTrainingCommand { get; internal set; }
        public ReactiveCommand<Unit, Unit> ConfigureTuningCommand { get; internal set; }
    }
}
