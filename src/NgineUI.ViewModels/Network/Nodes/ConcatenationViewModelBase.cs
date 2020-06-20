using DynamicData;
using DynamicData.Binding;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class ConcatenationViewModelBase<TLayer, TSensor> : NgineNodeViewModel
    {
        private const string NameBase = "Concatenation";

        public ValueListNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public ConcatenationViewModelBase(LayerIdTracker idTracker, PortType port, bool setId)
            : base(idTracker, NodeType.Layer, CombineName(NameBase, port), setId)
        {
            Previous = new NgineListInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = Previous.Values.Connect().Select(_ =>
                {
                    foreach (var l in Previous.Values.Items)
                    {
                        UpdateId(l, DefaultPrevious);
                    }

                    return HeadLayer<TLayer>.NewHeadLayer(this.Id, EvaluateOutput(Previous.Values.Items.ToArray()));
                })
            };

            Output = new NgineOutputViewModel<NonHeadLayer<TLayer, TSensor>>(port)
            {
                Value = HeadOutput.Value.Select(o => NonHeadLayer<TLayer, TSensor>.NewLayer(o))
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        protected abstract TLayer EvaluateOutput(NonHeadLayer<TLayer, TSensor>[] layers);
        protected abstract TLayer DefaultPrevious { get; }
    }
}
