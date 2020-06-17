using DynamicData.Binding;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;

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
        private readonly bool setId;
        private Tuple<uint, uint> id;

        public Tuple<uint, uint> Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public NodeType NodeType { get; }

        public NgineNodeViewModel(LayerIdTracker idTracker, NodeType type, string name, bool setId)
        {
            this.idTracker = idTracker;
            this.setId = setId;
            this.WhenValueChanged(vm => vm.Id)
                .Where(id => id != null)
                .Subscribe(id => this.Name = name + (id.Item1 != 0 ? $" ({id.Item1}-{id.Item2})" : ""));

            Id = setId ? idTracker.Generate(0u) : Tuple.Create(0u, 0u);
            NodeType = type;
        }

        protected (Tuple<uint, uint> Id, NonHeadLayer<TLayer, TSensor> Prev) UpdateId<TLayer, TSensor>(
            NonHeadLayer<TLayer, TSensor> previous, TLayer layerFallback)
        {
            previous = DefaultIfNull(previous, layerFallback);
            var (prevId, _) = NetworkConverters.getLayerId(previous);
            
            if (prevId + 1u > Id.Item1 && setId)
            {
                this.Id = idTracker.Generate(prevId);
            }

            return (this.Id, previous);
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

        public abstract FSharpChoice<Head, HeadLayer, Sensor> GetValue();
    }
}
