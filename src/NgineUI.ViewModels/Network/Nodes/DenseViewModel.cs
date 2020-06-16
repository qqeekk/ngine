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
    public class DenseViewModel : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<string> UnitsEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Previous { get; }
        public ValueNodeOutputViewModel<HeadLayer<Layer1D>> HeadOutput { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>> Output { get; }

        public DenseViewModel(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, bool setId)
            : base(idTracker, NodeType.Layer, "Dense", setId)
        {
            var unitsEditor = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            UnitsEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Units",
                Editor = unitsEditor,
            };
            UnitsEditor.Port.IsVisible = false;
            this.Inputs.Add(UnitsEditor);

            Previous = new NgineInputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<Layer1D>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    Previous.ValueChanged.Select(p => UpdateId(p, Layer1D.Empty1D)),
                    UnitsEditor.ValueChanged.Select(_ => OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, unitsEditor.SelectedValue)),
                    (o, units) => HeadLayer<Layer1D>.NewHeadLayer(o.Id, Layer1D.NewDense(new Dense(units), o.Prev)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<Layer1D, Sensor1D>>(PortType.Layer1D)
            {
                Value = HeadOutput.Value.Select(NonHeadLayer<Layer1D, Sensor1D>.NewLayer)
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }
    }
}
