using DynamicData;
using DynamicData.Binding;
using Microsoft.FSharp.Core;
using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using static Ngine.Domain.Schemas.Schema;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class ConvViewModelBase<TVector, TLayer, TSensor> : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<string> FiltersEditor { get; }
        public ValueNodeInputViewModel<string> PaddingEditor { get; }
        public ValueNodeInputViewModel<TVector> KernelEditor { get; }
        public ValueNodeInputViewModel<TVector> StridesEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public ConvViewModelBase(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, NodeType type, PortType port, string name, bool setId)
            : base(idTracker, type, name, setId)
        {
            var filtersEditor = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            FiltersEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Filters",
                Editor = filtersEditor,
            };
            FiltersEditor.Port.IsVisible = false;
            this.Inputs.Add(FiltersEditor);

            KernelEditor = new ValueNodeInputViewModel<TVector>
            {
                Name = "Kernel",
                Editor = CreateVectorEditor(ambiguities),
            };
            KernelEditor.Port.IsVisible = false;
            this.Inputs.Add(KernelEditor);

            StridesEditor = new ValueNodeInputViewModel<TVector>
            {
                Name = "Strides",
                Editor = CreateVectorEditor(ambiguities),
            };
            StridesEditor.Port.IsVisible = false;
            this.Inputs.Add(StridesEditor);

            PaddingEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Padding",
                Editor = new ComboEditorViewModel(PaddingEncoder.values),
            };
            PaddingEditor.Port.IsVisible = false;
            this.Inputs.Add(PaddingEditor);

            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            Previous.ValueChanged
                .Where(i => i != null)
                .Select(i => NetworkConverters.getLayerId(i).Item1)
                .Subscribe(prevId => this.Id = (prevId + 1u != Id.Item1) ? idTracker.Generate(prevId) : Id);

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    (this).WhenValueChanged(vm => vm.Id),
                    FiltersEditor.ValueChanged,
                    KernelEditor.ValueChanged,
                    StridesEditor.ValueChanged,
                    PaddingEditor.ValueChanged.Select(p => PaddingEncoder.tryParsePadding(p).Value),
                    (id, f, k, s, p) => HeadLayer<TLayer>.NewHeadLayer(id,
                        EvaluateOutput(OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, filtersEditor.SelectedValue), k, s, p)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<TLayer, TSensor>>(port)
            {
                Value = HeadOutput.Value.Select(o => NonHeadLayer<TLayer, TSensor>.NewLayer(o))
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        protected abstract ValueEditorViewModel<TVector> CreateVectorEditor(ObservableCollection<string> ambiguities);
        protected abstract TLayer EvaluateOutput(AmbiguousUIntViewModel filters, TVector kernel, TVector strides, Padding padding);
    }
}
