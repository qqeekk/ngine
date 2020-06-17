using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.AppServices.Abstract;
using NgineUI.ViewModels.Control;
using NgineUI.ViewModels.Network;
using NgineUI.ViewModels.Network.Nodes;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Ngine.Domain.Schemas.Schema;
using static NodeNetwork.Toolkit.NodeList.NodeListViewModel;

namespace NgineUI.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
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

        private static bool InvertIfTrue(ref bool flag)
        {
            if (flag)
            {
                flag = false;
                return true;
            }

            return false;
        }

        public NetworkViewModel Network { get; } = new NetworkViewModel();
        public NodeListViewModel NodeList { get; }
        public AmbiguitiesViewModel Ambiguities { get; }
        public HeaderViewModel Header { get; set; }

        public MainViewModel(INetworkPartsConverter partsConverter)
        {
            // TODO: inject
            var idTracker = new LayerIdTracker();
            var activatorConverter = ActivatorConverter.instance;
            var lossConverter = LossConverter.instance;
            var ambiguityConverter = AmbiguityConverter.instance;

            // Set up ambiguity values.
            // TODO: remove these
            var ambiguities = new ObservableCollection<string>
            { 
                "aaa",
                "bbb"
            };

            var ambiguityValues = new ObservableCollection<Ambiguity>
            {
                ambiguityConverter.Encode(KeyValuePair.Create(
                    AmbiguityVariableName.NewVariable(ambiguities[0]),
                    Values<uint>.NewList(new[]{ 3u, 5u }))),

                ambiguityConverter.Encode(KeyValuePair.Create(
                    AmbiguityVariableName.NewVariable(ambiguities[1]),
                    Values<uint>.NewRange(new Range<uint>(84u, 4u, 100u))))
            };

            Ambiguities = new AmbiguitiesViewModel
            {
                Items = ambiguityValues
            };

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

            Header = new HeaderViewModel(Network, Ambiguities, partsConverter);

            NodeList.AddNodeType(() => new Input1DViewModel(idTracker, !InvertIfTrue(ref input1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Input2DViewModel(idTracker, !InvertIfTrue(ref input2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Input3DViewModel(idTracker, !InvertIfTrue(ref input3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new DenseViewModel(idTracker, ambiguities, !InvertIfTrue(ref denseViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Conv2DViewModel(idTracker, ambiguities, !InvertIfTrue(ref conv2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Conv3DViewModel(idTracker, ambiguities, !InvertIfTrue(ref conv3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Pooling2DViewModel(idTracker, ambiguities, !InvertIfTrue(ref pooling2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Pooling3DViewModel(idTracker, ambiguities, !InvertIfTrue(ref pooling3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation1DViewModel(activatorConverter, idTracker, !InvertIfTrue(ref activation1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation2DViewModel(activatorConverter, idTracker, !InvertIfTrue(ref activation2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Activation3DViewModel(activatorConverter, idTracker, !InvertIfTrue(ref activation3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation1DViewModel(idTracker, !InvertIfTrue(ref concatenation1DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation2DViewModel(idTracker, !InvertIfTrue(ref concatenation2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Concatenation3DViewModel(idTracker, !InvertIfTrue(ref concatenation3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Flatten2DViewModel(idTracker, !InvertIfTrue(ref flatten2DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Flatten3DViewModel(idTracker, !InvertIfTrue(ref flatten3DViewModelIsFirstLoaded)));
            NodeList.AddNodeType(() => new Head1DViewModel(activatorConverter, lossConverter));
            NodeList.AddNodeType(() => new Head2DViewModel(activatorConverter, lossConverter));
            NodeList.AddNodeType(() => new Head3DViewModel(activatorConverter, lossConverter));

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
    }
}
