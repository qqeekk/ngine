using Microsoft.FSharp.Core;
using Ngine.Infrastructure.Abstractions;
using Ngine.Infrastructure.Abstractions.Services;
using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels.Parameters
{
    using TuneParametersResult = FSharpResult<(string path, uint epochs, float validationSplit), string[]>;

    public class TuneParametersViewModel: ReactiveObject, IInteractable
    {
        public TuneParametersViewModel(IInteractionService interactionService, IFileFormat mappingsFileFormat)
        {
            var dataMappings = new DataMappingsViewModel(interactionService, mappingsFileFormat);
            ConfigureDataMappingsCommand = ReactiveCommand.Create(() =>
            {
                interactionService.Navigate(dataMappings, "Ngine - Привязки");
            });

            SaveConfigurationCommand = ReactiveCommand.Create(() =>
            {
                (this as IInteractable).FinishInteraction();
            });

            EpochsValueEditor = new UIntEditorViewModel();
            ValidationSplitEditorViewModel = new FloatEditorViewModel();

            // Trick to propagate event from VM w/o exposing it to public API.
            dataMappings.WhenAnyValue(d => d.DataMappingsPath)
                .BindTo(this, vm => vm.DataMappingsPath);
        }

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
        Action IInteractable.FinishInteraction { get; set; }

        public TuneParametersResult TryGetValue()
        {
            if (OptionModule.IsNone(DataMappingsPath))
            {
                return TuneParametersResult.NewError(new[] { "Не указан путь до файла отображений" });
            }

            return TuneParametersResult.NewOk((
                DataMappingsPath.Value,
                EpochsValueEditor.Value,
                ValidationSplitEditorViewModel.Value));
        }
    }
}
