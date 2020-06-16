using DynamicData;
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
    public abstract class PoolingViewModelBase<TVector, TLayer, TSensor> : NgineNodeViewModel
    {
        public ValueNodeInputViewModel<string> PoolingEditor { get; }
        public ValueNodeInputViewModel<TVector> KernelEditor { get; }
        public ValueNodeInputViewModel<TVector> StridesEditor { get; }
        public ValueNodeInputViewModel<NonHeadLayer<TLayer, TSensor>> Previous { get; }
        public ValueNodeOutputViewModel<NonHeadLayer<TLayer, TSensor>> Output { get; }
        public ValueNodeOutputViewModel<HeadLayer<TLayer>> HeadOutput { get; }

        public PoolingViewModelBase(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, NodeType type, PortType port, string name, bool setId)
            : base(idTracker, type, name, setId)
        {
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

            PoolingEditor = new ValueNodeInputViewModel<string>
            {
                Name = "Pooling",
                Editor = new ComboEditorViewModel(PoolingTypeEncoder.values),
            };
            PoolingEditor.Port.IsVisible = false;
            this.Inputs.Add(PoolingEditor);

            Previous = new NgineInputViewModel<NonHeadLayer<TLayer, TSensor>>(port);
            this.Inputs.Add(Previous);

            HeadOutput = new NgineOutputViewModel<HeadLayer<TLayer>>(PortType.Head)
            {
                Value = Observable.CombineLatest(
                    Previous.ValueChanged.Select(p => UpdateId(p, DefaultPrevious)),
                    KernelEditor.ValueChanged,
                    StridesEditor.ValueChanged,
                    PoolingEditor.ValueChanged.Select(p => PoolingTypeEncoder.tryParsePoolingType(p).Value),
                    (o, k, s, p) => HeadLayer<TLayer>.NewHeadLayer(o.Id, EvaluateOutput(o.Prev, k, s, p)))
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
        protected abstract TLayer EvaluateOutput(NonHeadLayer<TLayer, TSensor> prev, TVector kernel, TVector strides, PoolingType pooling);
    }
}
