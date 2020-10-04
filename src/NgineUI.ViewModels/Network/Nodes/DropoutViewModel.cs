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
    public class DropoutViewModel : NgineNodeViewModel, IConfigurable<float>
    {
        public FloatEditorViewModel RateEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Previous { get; }
        public ValueNodeOutputViewModel<HeadLayer<Layer1D>> HeadOutput { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Output { get; }

        public DropoutViewModel(LayerIdTracker idTracker, bool setId)
            : base(idTracker, NodeType.Layer, "Dropout", setId)
        {
            RateEditor = new FloatEditorViewModel();
            AddInlinedInput("Rate", RateEditor);

            Previous = new NgineInputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<Layer1D>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    shouldUpdateChanged,
                    Previous.ValueChanged.Select(p => UpdateId(p, Layer1D.Empty1D)),
                    RateEditor.ValueChanged.Select(_ => RateEditor.Value),
                    (_, prev, rate) => HeadLayer<Layer1D>.NewHeadLayer(Id, Layer1D.NewDropout(rate, prev)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D)
            {
                Value = HeadOutput.Value.Select(NonHeadLayer<Layer1D, Sensor1D>.NewLayer)
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD1(HeadOutput.CurrentValue));

        public void Setup(float config)
        {
            RateEditor.Value = config;
        }
    }
}
