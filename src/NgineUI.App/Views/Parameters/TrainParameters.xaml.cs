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
                this.BindCommand(ViewModel, vm => vm.ConfigureDataMappingsCommand, v => v.btnDataMappings).DisposeWith(d);
                ViewModel.ConfigureDataMappingsShouldOpen.Subscribe(_ => ShowDataMappingsWindow()).DisposeWith(d);
            });
        }

        private void ShowDataMappingsWindow()
        {
            var dataMappings = new Window
            {
                Height = 700,
                Width = 1000,

                Title = "Ngine - Привязки",
                Content = new DataMappings(),
            };
            dataMappings.ShowDialog();
        }
    }
}
