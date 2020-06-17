using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Input1DViewModel : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<uint> InputsEditor { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Output { get; }

        public Input1DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, NodeType.Input, "Input1D", setId)
        {
            InputsEditor = new ValueNodeInputViewModel<uint>
            {
                Name = "Inputs",
                Editor = new UIntEditorViewModel(),
            };
            InputsEditor.Port.IsVisible = false;
            this.Inputs.Add(InputsEditor);

            Output = new NgineOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D)
            {
                Value = InputsEditor.ValueChanged.Select(v => NonHeadLayer<Layer1D, Sensor1D>.NewSensor(Id, new Sensor1D(v)))
            };

            this.Outputs.Add(Output);
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => SensorChoice(Sensor.NewSensor1D(Id, (Output.CurrentValue as NonHeadLayer<Layer1D, Sensor1D>.Sensor).Item2));
    }
}