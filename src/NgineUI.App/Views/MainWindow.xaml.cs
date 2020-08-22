using Ngine.Backend.Converters;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.Serialization;
using Ngine.Infrastructure.Services;
using NgineUI.ViewModels;
using NgineUI.ViewModels.Functional;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NgineUI.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Header, v => v.header.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Network, v => v.network.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NodeList, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Ambiguities, v => v.ambiguities.ViewModel).DisposeWith(d);
                ViewModel.ConversionErrorRaised.Subscribe(v => MessageBox.Show("Ошибка при загрузке схемы"));
            });

            var kernelConverter = KernelConverter.create(ActivatorConverter.instance);
            var networkConverter = NetworkConverters.create(kernelConverter,
                LossConverter.instance,
                OptimizerConverter.instance,
                AmbiguityConverter.instance);

            var networkIO = new InconsistentNetworkIO(networkConverter, SerializationProfile.Deserializer, SerializationProfile.Serializer);
            this.ViewModel = new MainViewModel(networkIO, NetworkViewModelManager.instance(networkConverter));
        }
    }
}
