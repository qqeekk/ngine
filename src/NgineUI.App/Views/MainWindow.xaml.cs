using Microsoft.FSharp.Core;
using Ngine.Backend;
using Ngine.Backend.Converters;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.Serialization;
using Ngine.Infrastructure.Services;
using Ngine.Infrastructure.Services.FileFormats;
using NgineUI.App.Views;
using NgineUI.App.Views.Parameters;
using NgineUI.ViewModels;
using NgineUI.ViewModels.Functional;
using NgineUI.ViewModels.Parameters;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;

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
                this.OneWayBind(ViewModel, vm => vm.Optimizer, v => v.optimizer.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CurrentFileName, v => v.Title, opt =>
                    $"Ngine{OptionModule.DefaultValue(string.Empty, OptionModule.Map(FSharpFunc<string, string>.FromConverter(f => " - " + f) , opt))}").DisposeWith(d);

                ViewModel.ConversionErrorRaised.Subscribe(v => MessageBox.Show("Ошибка при загрузке схемы")).DisposeWith(d);
            });

            var kernelConverter = KernelConverter.create(ActivatorConverter.instance);
            var networkConverter = NetworkConverters.create(kernelConverter,
                LossConverter.instance,
                OptimizerConverter.instance,
                AmbiguityConverter.instance);

            var kerasNetworkGenerator = new KerasNetworkGenerator(App.KerasOptions.PythonPath);

            var inconsistentNetworkIO = new InconsistentNetworkIO(networkConverter, SerializationProfile.Deserializer, SerializationProfile.Serializer);
            var networkIO = new NetworkIO(networkConverter, SerializationProfile.Deserializer, SerializationProfile.Serializer);
            var kerasNetworkCompiler = new KerasNetworkCompiler(SerializationProfile.Serializer, kerasNetworkGenerator);

            this.ViewModel = new MainViewModel(inconsistentNetworkIO,
                                               networkIO,
                                               kerasNetworkCompiler,
                                               NetworkViewModelManager.instance(networkConverter),
                                               new UIHelpers(),
                                               new NgineMappingsFormat(),
                                               App.KerasOptions.OutputDirectory);
        }
    }
}
