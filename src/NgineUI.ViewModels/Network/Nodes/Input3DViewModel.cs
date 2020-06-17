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
    using UIntVector3D = Tuple<uint, uint, uint>;

    public class Input3DViewModel : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<UIntVector3D> InputsEditor { get; }
        public ValueNodeInputViewModel<uint> ChannelsEditor { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer3D, Sensor3D>> Output { get; }

        public Input3DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, NodeType.Input, "Input3D", setId)
        {
            InputsEditor = new ValueNodeInputViewModel<UIntVector3D>
            {
                Name = "Inputs",
                Editor = new UIntVector3DEditorViewModel(),
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

            Output = new NgineOutputViewModel<NonHeadLayer<Layer3D, Sensor3D>>(PortType.Layer3D)
            {
                Value = Observable.CombineLatest(
                    InputsEditor.ValueChanged,
                    ChannelsEditor.ValueChanged, (i, c) =>
                        NonHeadLayer<Layer3D, Sensor3D>.NewSensor(Id, new Sensor3D(c, i))),
            };

            this.Outputs.Add(Output);
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => SensorChoice(Sensor.NewSensor3D(Id, (Output.CurrentValue as NonHeadLayer<Layer3D, Sensor3D>.Sensor).Item2));
    }
}