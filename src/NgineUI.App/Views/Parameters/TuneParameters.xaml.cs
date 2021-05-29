using Microsoft.FSharp.Core;
using NgineUI.ViewModels.Parameters;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Parameters
{
    /// <summary>
    /// Interaction logic for TuneParameters.xaml
    /// </summary>
    public partial class TuneParameters : IViewFor<TuneParametersViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(TuneParametersViewModel), typeof(TuneParameters), new PropertyMetadata(null));

        public TuneParametersViewModel ViewModel
        {
            get => (TuneParametersViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TuneParametersViewModel)value;
        }
        #endregion

        public TuneParameters()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.DataMappingsPath, v => v.txtDataMappingsPath.Text,
                    opt => OptionModule.DefaultValue(string.Empty, opt)).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.EpochsValueEditor, v => v.eEpochs.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TrialsValueEditor, v => v.eTrials.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ValidationSplitEditorViewModel, v => v.eValidationSplit.ViewModel).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveConfigurationCommand, v => v.btnOk).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ConfigureDataMappingsCommand, v => v.btnDataMappings).DisposeWith(d);
            });
        }
    }
}