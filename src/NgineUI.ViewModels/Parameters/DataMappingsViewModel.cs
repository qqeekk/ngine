using Microsoft.FSharp.Core;
using Microsoft.Win32;
using ReactiveUI;
using System.IO;
using System.Reactive.Linq;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels.Parameters
{
    public class DataMappingsViewModel : ReactiveObject
    {
        private const string DefaultDataMappingsText = @" ### ПРИМЕР ###
# Определение отображений для набора данных https://github.com/emanhamed/Houses-dataset

files:
    csv: D:\projects\diploma\Ngine\docs\sample_data_header.csv
    images: D:\projects\diploma\Ngine\docs\images

inputs:
    -
        - cons:$csv[0:2] # количество спальных комнат, количество ванных комнат, площадь
        - cats:$csv[3] # почтовый индекс района
    -
        - img:$images # коллажи из 4 фотографий жилья
outputs:
    -
        - cons:$csv[4]  # цена жилья (в долларах)";

        public DataMappingsViewModel()
        {
            DataMappingsPath = FSharpOption<string>.None;
            LoadDataMappingsCommand = ReactiveCommand.Create(LoadDataMappings);
            SaveDataMappingsCommand = ReactiveCommand.Create(SaveDataMappings);

            this.WhenAnyValue(vm => vm.DataMappingsPath)
                .Select(opt => OptionModule.IsSome(opt)
                    ? File.ReadAllText(opt.Value)
                    : DefaultDataMappingsText)
                .BindTo(this, vm => vm.DataMappingsText);
        }

        #region DataMappingsPath
        private FSharpOption<string> dataMappingsPath;

        public FSharpOption<string> DataMappingsPath
        {
            get => dataMappingsPath;
            private set => this.RaiseAndSetIfChanged(ref dataMappingsPath, value);
        }
        #endregion

        #region DataMappingsText
        private string dataMappingsText;

        public string DataMappingsText
        {
            get { return dataMappingsText; }
            set { this.RaiseAndSetIfChanged(ref dataMappingsText, value); }
        }
        #endregion

        public ReactiveCommand<Unit, Unit> LoadDataMappingsCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveDataMappingsCommand { get; }

        private void SaveDataMappings()
        {
            if (OptionModule.IsNone(DataMappingsPath))
            {
                var fileDialog = new SaveFileDialog
                {
                    Filter = "Ngine-mappings files (*.yaml)|*.yaml",
                };

                switch (fileDialog.ShowDialog())
                {
                    case true:
                        DataMappingsPath = fileDialog.FileName;
                        break;

                    default:
                        return;
                }
            }

            File.WriteAllText(DataMappingsPath.Value, DataMappingsText);
        }

        private void LoadDataMappings()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Ngine-mappings files (*.yaml)|*.yaml",
            };

            if (fileDialog.ShowDialog() == true)
            {
                DataMappingsPath = fileDialog.FileName;
            }
        }
    }
}
