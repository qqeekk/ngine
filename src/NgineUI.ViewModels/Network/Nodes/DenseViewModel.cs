using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class DenseViewModel : NgineNodeViewModel, IConfigurable<Dense>
    {
        public AmbiguousUIntEditorViewModel UnitsEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Previous { get; }
        public ValueNodeOutputViewModel<HeadLayer<Layer1D>> HeadOutput { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Output { get; }

        public DenseViewModel(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, bool setId)
            : base(idTracker, NodeType.Layer, "Dense", setId)
        {
            UnitsEditor = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            AddInlinedInput("Units", UnitsEditor);

            Previous = new NgineInputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<Layer1D>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    Previous.ValueChanged.Select(p => UpdateId(p, Layer1D.Empty1D)),
                    UnitsEditor.ValueChanged.Select(_ => OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, UnitsEditor.SelectedValue)),
                    (o, units) => HeadLayer<Layer1D>.NewHeadLayer(o.Id, Layer1D.NewDense(new Dense(units), o.Prev)))
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

        public void Setup(Dense config)
        {
            UnitsEditor.Value = Ambiguous.stringify(config.Units);
        }
    }
}
