using Microsoft.FSharp.Core;
using NgineUI.App.Views.Network;
using NgineUI.ViewModels.Parameters;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NgineUI.App.Views.Parameters
{
    /// <summary>
    /// Interaction logic for TrainParameters.xaml
    /// </summary>
    public partial class TrainParameters : IViewFor<TrainParametersViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(TrainParametersViewModel), typeof(TrainParameters), new PropertyMetadata(null));

        public TrainParametersViewModel ViewModel
        {
            get => (TrainParametersViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TrainParametersViewModel)value;
        }
        #endregion

        public TrainParameters()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.DataMappingsPath, v => v.txtDataMappingsPath.Text,
                    opt => OptionModule.DefaultValue(string.Empty, opt)).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.BatchSizeValueEditor, v => v.eBatchSize.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.EpochsValueEditor, v => v.eEpochs.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ValidationSplitEditorViewModel, v => v.eValidationSplit.ViewModel).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ConfigureDataMappingsCommand, v => v.btnDataMappings).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveConfigurationCommand, v => v.btnOk).DisposeWith(d);

                ViewModel.ConfigureDataMappingsShouldOpen.Subscribe(vm => ShowDataMappingsWindow(vm)).DisposeWith(d);
            });
        }

        private void ShowDataMappingsWindow(DataMappingsViewModel viewModel)
        {
            var view = new DataMappings { ViewModel = viewModel };
            var dataMappings = UIHelpers.CreateWindow(view, "Ngine - Привязки");

            dataMappings.ShowDialog();
        }
    }
}
