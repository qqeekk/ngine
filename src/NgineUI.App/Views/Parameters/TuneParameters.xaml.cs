using NgineUI.ViewModels.Parameters;
using ReactiveUI;
using System;
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