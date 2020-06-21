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

    public class Input2DViewModel : NgineNodeViewModel, IConfigurable<Sensor2D>
    {
        public ValueEditorViewModel<UIntVector2D> InputsEditor { get; }
        public ValueEditorViewModel<uint> ChannelsEditor { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer2D, Sensor2D>> Output { get; }

        public Input2DViewModel(LayerIdTracker idTracker, bool setId)
            : base(idTracker, NodeType.Input, CombineName("Input", PortType.Layer2D), setId)
        {
            InputsEditor = new UIntVector2DEditorViewModel();
            AddInlinedInput("Inputs", InputsEditor);

            ChannelsEditor = new UIntEditorViewModel();
            AddInlinedInput("Channels", ChannelsEditor);

            Output = new NgineOutputViewModel<NonHeadLayer<Layer2D, Sensor2D>>(PortType.Layer2D)
            {
                Value = Observable.CombineLatest(
                    shouldUpdateChanged,
                    InputsEditor.ValueChanged,
                    ChannelsEditor.ValueChanged, (_, i, c) => NonHeadLayer<Layer2D, Sensor2D>.NewSensor(Id, new Sensor2D(c, i))),
            };

            this.Outputs.Add(Output);
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => SensorChoice(Sensor.NewSensor2D(Id, (Output.CurrentValue as NonHeadLayer<Layer2D, Sensor2D>.Sensor).Item2));

        public void Setup(Sensor2D config)
        {
            InputsEditor.Value = config.Inputs;
            ChannelsEditor.Value = config.Channels;
        }
    }
}