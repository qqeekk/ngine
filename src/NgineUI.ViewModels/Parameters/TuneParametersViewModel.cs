using Microsoft.FSharp.Core;
using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System.Reactive.Subjects;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels.Parameters
{
    public class TuneParametersViewModel: ReactiveObject
    {
        public TuneParametersViewModel()
        {
            var dataMappings = new DataMappingsViewModel();
            ConfigureDataMappingsShouldOpen = new Subject<DataMappingsViewModel>();
            ConfigurationSaved = new Subject<Unit>();
            ConfigureDataMappingsCommand = ReactiveCommand.Create(() => ConfigureDataMappingsShouldOpen.OnNext(dataMappings));
            SaveConfigurationCommand = ReactiveCommand.Create(() => ConfigurationSaved.OnNext(Unit.Default));

            EpochsValueEditor = new UIntEditorViewModel();
            ValidationSplitEditorViewModel = new FloatEditorViewModel();

            // Trick to propagate event from VM w/o exposing it to public API.
            dataMappings.WhenAnyValue(d => d.DataMappingsPath)
                .BindTo(this, vm => vm.DataMappingsPath);
        }


        public Subject<DataMappingsViewModel> ConfigureDataMappingsShouldOpen { get; }
        public Subject<Unit> ConfigurationSaved { get; }

        public ReactiveCommand<Unit, Unit> ConfigureDataMappingsCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveConfigurationCommand { get; }

        #region DataMappingsPath
        private FSharpOption<string> dataMappingsPath;

        public FSharpOption<string> DataMappingsPath
        {
            get { return dataMappingsPath; }
            private set { this.RaiseAndSetIfChanged(ref dataMappingsPath, value); }
        }
        #endregion

        public UIntEditorViewModel EpochsValueEditor { get; }

        public FloatEditorViewModel ValidationSplitEditorViewModel { get; }
    }
}
