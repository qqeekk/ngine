using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    using UIntVector3D = Tuple<uint, uint, uint>;

    public class Input3DViewModel : NgineNodeViewModel, IConfigurable<Sensor3D>
    {
        public ValueEditorViewModel<UIntVector3D> InputsEditor { get; }
        public ValueEditorViewModel<uint> ChannelsEditor { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer3D, Sensor3D>> Output { get; }

        public Input3DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, NodeType.Input, CombineName("Input", PortType.Layer3D), setId)
        {
            InputsEditor = new UIntVector3DEditorViewModel();
            AddInlinedInput("Inputs", InputsEditor);

            ChannelsEditor = new UIntEditorViewModel();
            AddInlinedInput("Channels", ChannelsEditor);
            
            Output = new NgineOutputViewModel<NonHeadLayer<Layer3D, Sensor3D>>(PortType.Layer3D)
            {
                Value = Observable.CombineLatest(
                    shouldUpdateChanged,
                    InputsEditor.ValueChanged,
                    ChannelsEditor.ValueChanged, (_, i, c) =>
                        NonHeadLayer<Layer3D, Sensor3D>.NewSensor(Id, new Sensor3D(c, i))),
            };

            this.Outputs.Add(Output);
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => SensorChoice(Sensor.NewSensor3D(Id, (Output.CurrentValue as NonHeadLayer<Layer3D, Sensor3D>.Sensor).Item2));

        public void Setup(Sensor3D config)
        {
            InputsEditor.Value = config.Inputs;
            ChannelsEditor.Value = config.Channels;
        }
    }
}