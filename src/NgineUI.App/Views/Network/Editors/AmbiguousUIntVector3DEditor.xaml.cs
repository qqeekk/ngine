using NgineUI.ViewModels.Network.Editors;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace NgineUI.App.Views.Network.Editors
{
    /// <summary>
    /// Interaction logic for AmbiguousUIntVector2DEditor.xaml
    /// </summary>
    public partial class AmbiguousUIntVector3DEditor : IViewFor<AmbiguousUIntVector3DEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AmbiguousUIntVector3DEditorViewModel), typeof(AmbiguousUIntVector3DEditor), new PropertyMetadata(null));

        public AmbiguousUIntVector3DEditorViewModel ViewModel
        {
            get => (AmbiguousUIntVector3DEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AmbiguousUIntVector3DEditorViewModel)value;
        }
        #endregion

        public AmbiguousUIntVector3DEditor()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.XEditorViewModel, v => v.xEditor.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.YEditorViewModel, v => v.yEditor.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ZEditorViewModel, v => v.zEditor.ViewModel).DisposeWith(d);
            });
        }
    }
}
