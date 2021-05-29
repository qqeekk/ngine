using NgineUI.ViewModels.Network;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media;

namespace NgineUI.App.Views.Network
{
    /// <summary>
    /// Interaction logic for NgineNode.xaml
    /// </summary>
    public partial class NgineNode : IViewFor<NgineNodeViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NgineNodeViewModel), typeof(NgineNode), new PropertyMetadata(null));

        public NgineNodeViewModel ViewModel
        {
            get => (NgineNodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NgineNodeViewModel)value;
        }
        #endregion

        public NgineNode()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                NodeView.ViewModel = this.ViewModel;
                Disposable.Create(() => NodeView.ViewModel = null).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.NodeType, v => v.NodeView.Background, ConvertNodeTypeToBrush).DisposeWith(d);
            });
        }

        private Brush ConvertNodeTypeToBrush(NodeType type)
        {
            return type switch
            {
                NodeType.Head => new SolidColorBrush(Color.FromRgb(0x9b, 0x00, 0x00)),
                NodeType.Layer => new SolidColorBrush(Color.FromRgb(0x49, 0x49, 0x49)),
                NodeType.Input => new SolidColorBrush(Color.FromRgb(0x00, 0x39, 0xcb)),
                _ => throw new Exception("Недопустимый тип слоя"),
            };
        }
    }
}
