using DynamicData;
using DynamicData.Binding;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class FlattenViewModelBase<TLayer, TSensor> : NgineNodeViewModel
    {
        private const string NameBase = "Flatten";

        public FlattenViewModelBase(LayerIdTracker idTracker, PortType port, bool setId)
            : base(idTracker, NodeType.Layer, CombineName(NameBase, port), setId)
        {
            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<Layer1D>>(PortType.Head)
            {
                Value = Previous.ValueChanged.Select(p => UpdateId(p, DefaultPrevious))
                    .Select(o => HeadLayer<Layer1D>.NewHeadLayer(o.Id, EvaluateOutput(o.Prev)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D)
            {
                Value = HeadOutput.Value.Select(NonHeadLayer<Layer1D, Sensor1D>.NewLayer)
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        public ValueNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<Layer1D>> HeadOutput { get; }

        protected abstract TLayer DefaultPrevious { get; }
        protected abstract Layer1D EvaluateOutput(NonHeadLayer<TLayer, TSensor> prev);

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD1(HeadOutput.CurrentValue));
    }
}
