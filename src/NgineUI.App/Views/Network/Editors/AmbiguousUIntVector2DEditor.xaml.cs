using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for AmbiguousUIntVector2DEditor.xaml
    /// </summary>
    public partial class AmbiguousUIntVector2DEditor : IViewFor<AmbiguousUIntVector2DEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AmbiguousUIntVector2DEditorViewModel), typeof(AmbiguousUIntVector2DEditor), new PropertyMetadata(null));

        public AmbiguousUIntVector2DEditorViewModel ViewModel
        {
            get => (AmbiguousUIntVector2DEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AmbiguousUIntVector2DEditorViewModel)value;
        }
        #endregion

        public AmbiguousUIntVector2DEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.XEditorViewModel, v => v.xEditor.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.YEditorViewModel, v => v.yEditor.ViewModel).DisposeWith(d);
            });
        }
    }
}
