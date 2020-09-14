using Microsoft.FSharp.Core;
using NgineUI.ViewModels.Parameters;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Parameters
{
    /// <summary>
    /// Interaction logic for DataMappings.xaml
    /// </summary>
    public partial class DataMappings : IViewFor<DataMappingsViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(DataMappingsViewModel), typeof(DataMappings), new PropertyMetadata(null));

        public DataMappingsViewModel ViewModel
        {
            get => (DataMappingsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DataMappingsViewModel)value;
        }
        #endregion

        public DataMappings()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.DataMappingsPath, v => v.txtDataMappingsPath.Text,
                    opt => OptionModule.DefaultValue(string.Empty, opt)).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.DataMappingsText, v => v.yamlEditor.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveDataMappingsCommand, v => v.btnSaveDataMappings).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LoadDataMappingsCommand, v => v.btnLoadDataMappings).DisposeWith(d);
            });
        }
    }
}
