using NgineUI.ViewModels.Control;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Control
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : IViewFor<HeaderViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(HeaderViewModel), typeof(Header), new PropertyMetadata(null));

        public HeaderViewModel ViewModel
        {
            get => (HeaderViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (HeaderViewModel)value;
        }
        #endregion
        public Header()
        {
            InitializeComponent();
         
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.SaveModelCommand, v => v.saveNodesItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ReadModelCommand, v => v.readNodesItem).DisposeWith(d);
            });
        }
    }
}
