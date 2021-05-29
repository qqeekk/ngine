using NgineUI.ViewModels.Network;
using NgineUI.ViewModels.Network.Ambiguities;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Network
{
    /// <summary>
    /// Interaction logic for Ambiguity.xaml
    /// </summary>
    public partial class Ambiguity : IViewFor<AmbiguityViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AmbiguityViewModel), typeof(Ambiguity), new PropertyMetadata(null));

        public AmbiguityViewModel ViewModel
        {
            get => (AmbiguityViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AmbiguityViewModel)value;
        }
        #endregion

        public Ambiguity()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Name, v => v.eAmbName.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ValueEditor, v => v.eAmbValue.ViewModel).DisposeWith(d);
            });
        }
    }
}
