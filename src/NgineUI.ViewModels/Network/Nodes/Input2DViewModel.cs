using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    using UIntVector2D = Tuple<uint, uint>;

    public class Input2DViewModel : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<UIntVector2D> InputsEditor { get; }
        public ValueNodeInputViewModel<uint> ChannelsEditor { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer2D, Sensor2D>> Output { get; }

        public Input2DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, NodeType.Input, "Input2D", setId)
        {
            InputsEditor = new ValueNodeInputViewModel<UIntVector2D>
            {
                Name = "Inputs",
                Editor = new UIntVector2DEditorViewModel(),
            };
            InputsEditor.Port.IsVisible = false;
            this.Inputs.Add(InputsEditor);

            ChannelsEditor = new ValueNodeInputViewModel<uint>
            {
                Name = "Channels",
                Editor = new UIntEditorViewModel(),
            };
            ChannelsEditor.Port.IsVisible = false;
            this.Inputs.Add(ChannelsEditor);

            Output = new NgineOutputViewModel<NonHeadLayer<Layer2D, Sensor2D>>(PortType.Layer2D)
            {
                Value = Observable.CombineLatest(
                    InputsEditor.ValueChanged,
                    ChannelsEditor.ValueChanged, (i, c) => NonHeadLayer<Layer2D, Sensor2D>.NewSensor(Id, new Sensor2D(c, i))),
            };

            this.Outputs.Add(Output);
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => SensorChoice(Sensor.NewSensor2D(Id, (Output.CurrentValue as NonHeadLayer<Layer2D, Sensor2D>.Sensor).Item2));
    }
}