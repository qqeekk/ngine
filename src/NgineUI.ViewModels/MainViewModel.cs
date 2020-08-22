using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using Ngine.Infrastructure.Services;
using NgineUI.ViewModels.AppServices.Abstract;
using NgineUI.ViewModels.Control;
using NgineUI.ViewModels.Network;
using NgineUI.ViewModels.Network.Nodes;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using YamlDotNet.Serialization;
using static NodeNetwork.Toolkit.NodeList.NodeListViewModel;

namespace NgineUI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        // TODO: remove
        private const string DefaultFileName = "file.yaml";

        private readonly INetworkIO<InconsistentNetwork> networkIO;
        private readonly INetworkPartsConverter partsConverter;
        private LayerIdTracker idTracker;
        private bool activation1DViewModelIsFirstLoaded = true;
        private bool activation2DViewModelIsFirstLoaded = true;
        private bool activation3DViewModelIsFirstLoaded = true;
        private bool concatenation1DViewModelIsFirstLoaded = true;
        private bool concatenation2DViewModelIsFirstLoaded = true;
        private bool concatenation3DViewModelIsFirstLoaded = true;
        private bool conv2DViewModelIsFirstLoaded = true;
        private bool conv3DViewModelIsFirstLoaded = true;
        private bool flatten2DViewModelIsFirstLoaded = true;
        private bool flatten3DViewModelIsFirstLoaded = true;
        private bool input1DViewModelIsFirstLoaded = true;
        private bool input2DViewModelIsFirstLoaded = true;
        private bool input3DViewModelIsFirstLoaded = true;
        private bool pooling2DViewModelIsFirstLoaded = true;
        private bool pooling3DViewModelIsFirstLoaded = true;
        private bool denseViewModelIsFirstLoaded = true;
        private NetworkViewModel network;
        private AmbiguitiesViewModel ambiguities;
        private Optimizer optimizer;

        private static bool InvertIfTrue(ref bool flag)
        {
            if (flag)
            {
                flag = false;
                return true;
            }

            return false;
        }

        public NetworkViewModel Network
        {
            get => network;
            set => this.RaiseAndSetIfChanged(ref network, value);
        }

        public AmbiguitiesViewModel Ambiguities
        {
            get => ambiguities;
            set => this.RaiseAndSetIfChanged(ref ambiguities, value);
        }

        public Optimizer Optimizer
        { 
            get => optimizer; 
            set => this.RaiseAndSetIfChanged(ref optimizer, value);
        }

        public NodeListViewModel NodeList { get; }
        public HeaderViewModel Header { get; }
        public Subject<Unit> ConversionErrorRaised { get; }

        public MainViewModel(INetworkIO<InconsistentNetwork> networkIO, INetworkPartsConverter partsConverter)
        {
            // TODO: inject
            this.networkIO = networkIO;
            this.partsConverter = partsConverter;
            this.idTracker = new LayerIdTracker();
            var networkConverter = networkIO.NetworkConverter;

            Network = new NetworkViewModel();
            Optimizer = Optimizer.NewSGD(1e-4f, new SGD(0, 0));

            // Set up ambiguity values.
            Ambiguities = new AmbiguitiesViewModel(networkConverter.AmbiguityConverter);

            // TODO: remove these
            Ambiguities.Add(KeyValuePair.Create(
                AmbiguityVariableName.NewVariable("bbb"),
                Values<uint>.NewRange(new Range<uint>(84u, 4u, 100u))));

            Ambiguities.Add(KeyValuePair.Create(
                AmbiguityVariableName.NewVariable("aaa"),
                Values<uint>.NewList(new[] { 3u, 5u })));

            NodeList = new NodeListViewModel
            {
                Title = "Добавить слой",
                EmptySearchText = "Поиск...",
                EmptyLabel = "Нет результатов, удовлетворяющих условиям поиска.",
                StringifyDisplayMode = mode => mode switch
                {
                    DisplayMode.List => "список",
                    DisplayMode.Tiles => "миниатюры",
                    _ => throw new Exception()
                }
            };

            Header = new HeaderViewModel
            {
                SaveModelCommand = ReactiveCommand.Create(SaveModel),
                ReadModelCommand = ReactiveCommand.Create(ReadModel),
            };

            NodeList.AddNodeType(() => new Input1DViewModel(idTracker, !InvertIfTrue(ref input1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Input2DViewModel(idTracker, !InvertIfTrue(ref input2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Input3DViewModel(idTracker, !InvertIfTrue(ref input3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new DenseViewModel(idTracker, Ambiguities.Names, !InvertIfTrue(ref denseViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Conv2DViewModel(idTracker, Ambiguities.Names, !InvertIfTrue(ref conv2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Conv3DViewModel(idTracker, Ambiguities.Names, !InvertIfTrue(ref conv3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Pooling2DViewModel(idTracker, Ambiguities.Names, !InvertIfTrue(ref pooling2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Pooling3DViewModel(idTracker, Ambiguities.Names, !InvertIfTrue(ref pooling3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation1DViewModel(networkConverter.LayerConverter.ActivatorConverter, idTracker, !InvertIfTrue(ref activation1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation2DViewModel(networkConverter.LayerConverter.ActivatorConverter, idTracker, !InvertIfTrue(ref activation2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation3DViewModel(networkConverter.LayerConverter.ActivatorConverter, idTracker, !InvertIfTrue(ref activation3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation1DViewModel(idTracker, !InvertIfTrue(ref concatenation1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation2DViewModel(idTracker, !InvertIfTrue(ref concatenation2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation3DViewModel(idTracker, !InvertIfTrue(ref concatenation3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Flatten2DViewModel(idTracker, !InvertIfTrue(ref flatten2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Flatten3DViewModel(idTracker, !InvertIfTrue(ref flatten3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Head1DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));
            NodeList.AddNodeType(() => new Head2DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));
            NodeList.AddNodeType(() => new Head3DViewModel(networkConverter.LayerConverter.ActivatorConverter, networkConverter.LossConverter));

            ConversionErrorRaised = new Subject<Unit>();
            //TODO: remove/uncomment. 
            //var codeObservable = eventNode.OnClickFlow.Values.Connect().Select(_ => new StatementSequence(eventNode.OnClickFlow.Values.Items));
            //codeObservable.BindTo(this, vm => vm.CodePreview.Code);
            //codeObservable.BindTo(this, vm => vm.CodeSim.Code);

            //ForceDirectedLayouter layouter = new ForceDirectedLayouter();
            //var config = new Configuration
            //{
            //    Network = Network,
            //};
            //AutoLayout = ReactiveCommand.Create(() => layouter.Layout(config, 10000));
            //StartAutoLayoutLive = ReactiveCommand.CreateFromObservable(() =>
            //    Observable.StartAsync(ct => layouter.LayoutAsync(config, ct)).TakeUntil(StopAutoLayoutLive)
            //);
            //StopAutoLayoutLive = ReactiveCommand.Create(() => { }, StartAutoLayoutLive.IsExecuting);
        }


        private void SaveModel()
        {
            var encoded = partsConverter.Encode(Network, Ambiguities, Optimizer);
            networkIO.Write(DefaultFileName, encoded);
        }


        private void ReadModel()
        {
            if (networkIO.Read(DefaultFileName, out var network))
            {
                (Network, Ambiguities, Optimizer, idTracker) = partsConverter.Decode(network);
            }
            else
            {
                ConversionErrorRaised.OnNext(Unit.Default);
            }
        }
    }
}
