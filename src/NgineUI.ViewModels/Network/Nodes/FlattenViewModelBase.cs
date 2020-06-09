using DynamicData;
using DynamicData.Binding;
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
        public FlattenViewModelBase(LayerIdTracker idTracker, PortType port, string name, bool setId)
            : base(idTracker, NodeType.Layer, name, setId)
        {
            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            Previous.ValueChanged
                .Where(i => i != null)
                .Select(i => NetworkConverters.getLayerId(i).Item1)
                .Subscribe(prevId => this.Id = (prevId + 1u != Id.Item1) ? idTracker.Generate(prevId) : Id);

            
            HeadOutput = new NgineOutputViewModel<HeadLayer<Layer1D>>(PortType.Head)
            {
                Value = this.WhenValueChanged(vm => vm.Id)
                    .Select(id => HeadLayer<Layer1D>.NewHeadLayer(id, EvaluateOutput()))
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

        protected abstract Layer1D EvaluateOutput();
    }
}
