using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.ObjectModel;

namespace NgineUI.ViewModels.Network.Nodes
{
    using Ambiguous2DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>>;
    public class Pooling2DViewModel : PoolingViewModelBase<Ambiguous2DTuple, Layer2D, Sensor2D>
    {
        public Pooling2DViewModel(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer2D, "Pooling2D", setId)
        {
        }

        protected override Layer2D DefaultPrevious => Layer2D.Empty2D;

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD2(HeadOutput.CurrentValue));

        protected override ValueEditorViewModel<Ambiguous2DTuple> CreateVectorEditor(ObservableCollection<string> ambiguities)
            => new AmbiguousUIntVector2DEditorViewModel(ambiguities);

        protected override Layer2D EvaluateOutput(NonHeadLayer<Layer2D, Sensor2D> prev, Ambiguous2DTuple kernel, Ambiguous2DTuple strides, PoolingType pooling)
            => Layer2D.NewPooling2D(new Pooling2D(kernel, strides, pooling), prev);
    }
}
