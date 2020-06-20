using DynamicData;
using Microsoft.FSharp.Core;
using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class ConvViewModelBase<TVector, TLayer, TSensor> : NgineNodeViewModel, IConfigurable<Convolutional<TVector>>
    {
        private const string NameBase = "Conv";

        public AmbiguousUIntEditorViewModel FiltersEditor { get; }
        public ValueEditorViewModel<string> PaddingEditor { get; }
        public ValueEditorViewModel<TVector> KernelEditor { get; }
        public ValueEditorViewModel<TVector> StridesEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public ConvViewModelBase(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, NodeType type, PortType port, bool setId)
            : base(idTracker, type, CombineName(NameBase, port), setId)
        {
            FiltersEditor = new AmbiguousUIntEditorViewModel(0.ToString(), ambiguities);
            AddInlinedInput("Filters", FiltersEditor);

            KernelEditor = CreateVectorEditor(ambiguities);
            AddInlinedInput("Kernel", KernelEditor);

            StridesEditor = CreateVectorEditor(ambiguities);
            AddInlinedInput("Strides", StridesEditor);

            PaddingEditor = new ComboEditorViewModel(PaddingEncoder.values);
            AddInlinedInput("Padding", PaddingEditor);
            
            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    Previous.ValueChanged.Select(p => UpdateId(p, DefaultPrevious)),
                    FiltersEditor.ValueChanged,
                    KernelEditor.ValueChanged,
                    StridesEditor.ValueChanged,
                    PaddingEditor.ValueChanged.Select(p => PaddingEncoder.tryParsePadding(p).Value),
                    (o, f, k, s, p) => HeadLayer<TLayer>.NewHeadLayer(o.Id,
                        EvaluateOutput(o.Prev, OptionModule.DefaultValue(AmbiguousUIntViewModel.Default, FiltersEditor.SelectedValue), k, s, p)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<TLayer, TSensor>>(port)
            {
                Value = HeadOutput.Value.Select(o => NonHeadLayer<TLayer, TSensor>.NewLayer(o))
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        protected abstract TLayer DefaultPrevious { get; }
        protected abstract ValueEditorViewModel<TVector> CreateVectorEditor(ObservableCollection<string> ambiguities);
        protected abstract TLayer EvaluateOutput(NonHeadLayer<TLayer, TSensor> prev, AmbiguousUIntViewModel filters, TVector kernel, TVector strides, Padding padding);

        public void Setup(Convolutional<TVector> config)
        {
            FiltersEditor.Value = Ambiguous.stringify(config.Filters);
            KernelEditor.Value = config.Kernel;
            StridesEditor.Value = config.Strides;
            PaddingEditor.Value = PaddingEncoder.encoder.encode.Invoke(config.Padding);
        }
    }
}
