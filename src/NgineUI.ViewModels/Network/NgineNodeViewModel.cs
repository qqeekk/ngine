using DynamicData;
using DynamicData.Binding;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Unit = System.Reactive.Unit;

namespace NgineUI.ViewModels.Network
{
    public enum NodeType
    {
        Layer,
        Head,
        Input,
    }

    public abstract class NgineNodeViewModel : NodeViewModel
    {
        private readonly LayerIdTracker idTracker;
        private bool setId;

        public Tuple<uint, uint> Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }
        private Tuple<uint, uint> id;

        public NodeType NodeType { get; }

        protected readonly BehaviorSubject<bool> shouldUpdateChanged;

        public NgineNodeViewModel(LayerIdTracker idTracker, NodeType type, string name, bool setId)
        {
            this.idTracker = idTracker;
            this.setId = setId;
            this.shouldUpdateChanged = new BehaviorSubject<bool>(false);
            this.WhenValueChanged(vm => vm.Id)
                .Where(id => id != null)
                .Subscribe(id => this.Name = name + (id.Item1 != 0 ? $" ({id.Item1}-{id.Item2})" : ""));

            Id = setId ? idTracker.Generate(0u) : Tuple.Create(0u, 0u);
            NodeType = type;
        }

        public void EnableIdGenerator()
        {
            setId = true;
            shouldUpdateChanged.OnNext(true);
        }

        protected NonHeadLayer<TLayer, TSensor> UpdateId<TLayer, TSensor>(
            NonHeadLayer<TLayer, TSensor> previous, TLayer layerFallback)
        {
            previous = DefaultIfNull(previous, layerFallback);
            var (prevId, _) = NetworkConverters.getLayerId(previous);
            
            if (prevId + 1u > Id.Item1 && setId)
            {
                this.Id = idTracker.Generate(prevId);
            }

            return previous;
        }

        protected static HeadLayer<TLayer> WrapEmpty<TLayer>(TLayer empty) =>
            HeadLayer<TLayer>.NewHeadLayer(Tuple.Create(0u, 0u), empty);

        private static NonHeadLayer<TLayer, TSensor> WrapNonHead<TLayer, TSensor>(HeadLayer<TLayer> layer) =>
            NonHeadLayer<TLayer, TSensor>.NewLayer(layer);

        private static NonHeadLayer<TLayer, TSensor> DefaultIfNull<TLayer, TSensor>(NonHeadLayer<TLayer, TSensor> layer, TLayer @default) =>
            layer ?? WrapNonHead<TLayer, TSensor>(WrapEmpty(@default));

        protected static FSharpChoice<Head, HeadLayer, Sensor> HeadLayerChoice(HeadLayer node) =>
            FSharpChoice<Head, HeadLayer, Sensor>.NewChoice2Of3(node);

        protected static FSharpChoice<Head, HeadLayer, Sensor> SensorChoice(Sensor node) =>
            FSharpChoice<Head, HeadLayer, Sensor>.NewChoice3Of3(node);
        protected static FSharpChoice<Head, HeadLayer, Sensor> HeadChoice(Head node) =>
            FSharpChoice<Head, HeadLayer, Sensor>.NewChoice1Of3(node);

        protected static string CombineName(string @base, PortType? outPort = null)
        {
            return @base + outPort switch
            {
                PortType.Layer3D => "3D",
                PortType.Layer2D => "2D",
                PortType.Layer1D => "1D",
                _ => "",
            };
        }

        protected virtual void AddInlinedInput<TValue>(string name, ValueEditorViewModel<TValue> editor)
        {
            var input = new ValueNodeInputViewModel<TValue>
            {
                Name = name,
                Editor = editor,
            };
            input.Port.IsVisible = false;
            this.Inputs.Add(input);
        }

        public abstract FSharpChoice<Head, HeadLayer, Sensor> GetValue();
    }
}
