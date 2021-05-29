using Microsoft.FSharp.Core;
using Ngine.Infrastructure.Abstractions;
using Ngine.Infrastructure.Abstractions.Services;
using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels.Parameters
{
    using TrainParametersResult = FSharpResult<(string path, uint batch, uint epochs, float validationSplit), string[]>;

    public class TrainParametersViewModel : ReactiveObject, IInteractable
    {        
        public TrainParametersViewModel(IInteractionService interactionService, IFileFormat mappingsFileFormat)
        {
            var dataMappings = new DataMappingsViewModel(interactionService, mappingsFileFormat);

            ConfigureDataMappingsCommand = ReactiveCommand.Create(() => 
            {
                interactionService.Navigate(dataMappings, "Ngine - привязки");
            });

            SaveConfigurationCommand = ReactiveCommand.Create(() =>
            {
                (this as IInteractable).FinishInteraction();
            });

            BatchSizeValueEditor = new UIntEditorViewModel();
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

        public UIntEditorViewModel BatchSizeValueEditor { get; }

        public UIntEditorViewModel EpochsValueEditor { get; }

        public FloatEditorViewModel ValidationSplitEditorViewModel { get; }

        Action IInteractable.FinishInteraction { get; set; }

        public TrainParametersResult TryGetValue()
        {
            if (OptionModule.IsNone(DataMappingsPath))
            {
                return TrainParametersResult.NewError(new[] { "Не указан путь до файла отображений" });
            }

            return TrainParametersResult.NewOk((
                DataMappingsPath.Value,
                BatchSizeValueEditor.Value,
                EpochsValueEditor.Value,
                ValidationSplitEditorViewModel.Value));
        }
    }
}
