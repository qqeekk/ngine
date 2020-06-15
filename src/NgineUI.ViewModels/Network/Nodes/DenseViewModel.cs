using DynamicData;
using DynamicData.Binding;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using static Ngine.Domain.Schemas.Schema;

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

            Previous.ValueChanged
                .Where(i => i != null)
                .Select(i => NetworkConverters.getLayerId(i).Item1)
                .Subscribe(prevId => this.Id = (prevId + 1u != Id.Item1) ? idTracker.Generate(prevId) : Id);

            HeadOutput = new NgineOutputViewModel<HeadLayer<Layer1D>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    this.WhenValueChanged(vm => vm.Id),
                    UnitsEditor.ValueChanged.Select(_ => OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, unitsEditor.SelectedValue)),
                    (id, units) => HeadLayer<Layer1D>.NewHeadLayer(id,
                        Layer1D.NewDense(
                            new Dense(units),
                            Previous.Value
                                ?? NonHeadLayer<Layer1D, Sensor1D>.NewLayer(
                                    HeadLayer<Layer1D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer1D.Empty1D)))))
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
