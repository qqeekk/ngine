using DynamicData;
using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Ambiguities;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System.Reactive.Linq;

namespace NgineUI.ViewModels.Network.Nodes
{
    public abstract class PoolingViewModelBase<TVector, TLayer, TSensor> : NgineNodeViewModel, IConfigurable<Pooling<TVector>>
    {
        private const string NameBase = "Pooling";
        public ValueEditorViewModel<string> PoolingEditor { get; }
        public ValueEditorViewModel<TVector> KernelEditor { get; }
        public ValueEditorViewModel<TVector> StridesEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public PoolingViewModelBase(
            LayerIdTracker idTracker,
            AmbiguityListViewModel ambiguities,
            NodeType type,
            PortType port, 
            bool setId)
            : base(idTracker, type, CombineName(NameBase, port), setId)
        {
            KernelEditor = CreateVectorEditor(ambiguities);
            AddInlinedInput("Kernel", KernelEditor);

            StridesEditor = CreateVectorEditor(ambiguities);
            AddInlinedInput("Strides", StridesEditor);

            PoolingEditor = new ComboEditorViewModel(PoolingTypeEncoder.values);
            AddInlinedInput("Pooling", PoolingEditor);

            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    shouldUpdateChanged,
                    Previous.ValueChanged.Select(p => UpdateId(p, DefaultPrevious)),
                    KernelEditor.ValueChanged,
                    StridesEditor.ValueChanged,
                    PoolingEditor.ValueChanged.Select(p => PoolingTypeEncoder.tryParsePoolingType(p).Value),
                    (_, prev, k, s, p) => HeadLayer<TLayer>.NewHeadLayer(Id, EvaluateOutput(prev, k, s, p)))
            };

            Output = new NgineOutputViewModel<NonHeadLayer<TLayer, TSensor>>(port)
            {
                Value = HeadOutput.Value.Select(o => NonHeadLayer<TLayer, TSensor>.NewLayer(o))
            };

            this.Outputs.Add(Output);
            this.Outputs.Add(HeadOutput);
        }

        public void Setup(Pooling<TVector> config)
        {
            KernelEditor.Value = config.Kernel;
            StridesEditor.Value = config.Strides;
            PoolingEditor.Value = PoolingTypeEncoder.encoder.encode.Invoke(config.PoolingType);
        }

        protected abstract TLayer DefaultPrevious { get; }
        protected abstract ValueEditorViewModel<TVector> CreateVectorEditor(AmbiguityListViewModel ambiguities);
        protected abstract TLayer EvaluateOutput(NonHeadLayer<TLayer, TSensor> prev, TVector kernel, TVector strides, PoolingType pooling);
    }
}
