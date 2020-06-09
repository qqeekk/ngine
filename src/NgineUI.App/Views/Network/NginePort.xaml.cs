using NgineUI.ViewModels.Network.Connections;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NgineUI.App.Views.Network
{
    /// <summary>
    /// Interaction logic for NginePort.xaml
    /// </summary>
    public partial class NginePort : IViewFor<NginePortViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NginePortViewModel), typeof(NginePort), new PropertyMetadata(null));

        public NginePortViewModel ViewModel
        {
            get => (NginePortViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NginePortViewModel)value;
        }
        #endregion

        #region Template Resource Keys
        public const string Layer1DPortTemplateKey = "Layer1DPortTemplateKey";
        public const string Layer2DPortTemplateKey = "Layer2DPortTemplateKey";
        public const string Layer3DPortTemplateKey = "Layer3DPortTemplateKey";
        public const string HeadPortTemplateKey = "HeadPortTemplateKey";
        #endregion

        public NginePort()
        {
            InitializeComponent();

            this.WhenActivated(d => 
            {
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.PortView.ViewModel).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.PortType, v => v.PortView.Template, GetTemplateFromPortType)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IsMirrored, v => v.PortView.RenderTransform,
                    isMirrored => new ScaleTransform(isMirrored ? -1.0 : 1.0, 1.0))
                    .DisposeWith(d);
            });
        }

        public ControlTemplate GetTemplateFromPortType(PortType type)
        {
            switch (type)
            {
                case PortType.Layer1D:
                    return (ControlTemplate)Resources[Layer1DPortTemplateKey];
                case PortType.Layer2D:
                    return (ControlTemplate)Resources[Layer2DPortTemplateKey];
                case PortType.Layer3D:
                    return (ControlTemplate)Resources[Layer3DPortTemplateKey];
                case PortType.Head:
                    return (ControlTemplate)Resources[HeadPortTemplateKey];
                default:
                    throw new Exception("Unsupported port type");
            };
        }
    }
}
