using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NgineUI.App.Views.Parameters
{
    /// <summary>
    /// Interaction logic for DataMappings.xaml
    /// </summary>
    public partial class DataMappings : UserControl
    {
        private const string SampleText =
@"# Определение отображений для набора данных https://github.com/emanhamed/Houses-dataset

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

        public DataMappings()
        {
            InitializeComponent();
            yamlEditor.Document.Blocks.Add(new Paragraph(new Run(SampleText)));

        }
    }
}
