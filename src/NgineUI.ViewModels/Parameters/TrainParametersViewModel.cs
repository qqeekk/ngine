using ReactiveUI;
using System.Reactive;
using System.Reactive.Subjects;

namespace NgineUI.ViewModels.Parameters
{
    public class TrainParametersViewModel
    {
        public Subject<Unit> ConfigureDataMappingsShouldOpen { get; }
        public ReactiveCommand<Unit, Unit> ConfigureDataMappingsCommand { get; }

        public TrainParametersViewModel()
        {
            ConfigureDataMappingsShouldOpen = new Subject<Unit>();
            ConfigureDataMappingsCommand = ReactiveCommand.Create(ConfigureDataMappings);
        }

        private void ConfigureDataMappings()
        {
            ConfigureDataMappingsShouldOpen.OnNext(Unit.Default);
        }
    }
}
