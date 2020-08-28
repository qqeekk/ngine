using NgineUI.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Network
{
    /// <summary>
    /// Interaction logic for Optimizer.xaml
    /// </summary>
    public partial class Optimizer : IViewFor<OptimizerViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(OptimizerViewModel), typeof(Optimizer), new PropertyMetadata(null));

        public OptimizerViewModel ViewModel
        {
            get => (OptimizerViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (OptimizerViewModel)value;
        }
        #endregion
        public Optimizer()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ValueEditor, v => v.eOptimizer.ViewModel).DisposeWith(d);
            });
        }
    }
}
