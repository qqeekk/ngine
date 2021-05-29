using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network;
using NgineUI.ViewModels.Network.Ambiguities;
using ReactiveUI;
using System;
using System.Collections;
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

namespace NgineUI.App.Views.Network
{
    /// <summary>
    /// Interaction logic for AmbiguitiesView.xaml
    /// </summary>
    public partial class Ambiguities : IViewFor<AmbiguitiesViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AmbiguitiesViewModel), typeof(Ambiguities), new PropertyMetadata(null));

        public AmbiguitiesViewModel ViewModel
        {
            get => (AmbiguitiesViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AmbiguitiesViewModel)value;
        }
        #endregion

        public Ambiguities()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.CurrentAmbiguity, v => v.cAmbiguity.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AddAmbiguityCommand, v => v.btnAdd.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AmbiguityList.Items, v => v.lvAmbiguities.ItemsSource).DisposeWith(d);
            });
        }
    }
}
