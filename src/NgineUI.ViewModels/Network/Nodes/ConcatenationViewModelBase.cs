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
        public ValueListNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public ConcatenationViewModelBase(LayerIdTracker idTracker, PortType port, string name, bool setId)
            : base(idTracker, NodeType.Layer, name, setId)
        {
            Previous = new NgineListInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            Previous.Values.Connect().Subscribe(_ =>
            {
                var maxLevel = Previous.Values.Items
                    .Where(i => i != null)
                    .Select(i => NetworkConverters.getLayerId(i).Item1)
                    .DefaultIfEmpty(0u)
                    .Max();

                if (maxLevel + 1 != Id.Item1)
                {
                    Id = idTracker.Generate(maxLevel);
                }
            });

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = this.WhenValueChanged(vm => vm.Id).Select(id =>
                    HeadLayer<TLayer>.NewHeadLayer(id, EvaluateOutput(Previous.Values.Items.ToArray())))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<TLayer, TSensor>>(port)
            {
                Value = HeadOutput.Value.Select(o => NonHeadLayer<TLayer, TSensor>.NewLayer(o))
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        protected abstract TLayer EvaluateOutput(NonHeadLayer<TLayer, TSensor>[] layers);
    }
}
