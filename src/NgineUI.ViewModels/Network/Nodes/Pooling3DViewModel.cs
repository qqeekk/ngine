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
    using Ambiguous3DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>, Ambiguous<uint>>;

    public class Pooling3DViewModel : PoolingViewModelBase<Ambiguous3DTuple, Layer3D, Sensor3D>
    {
        public Pooling3DViewModel(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer3D, setId)
        {
        }

        protected override Layer3D DefaultPrevious => Layer3D.Empty3D;

        protected override ValueEditorViewModel<Ambiguous3DTuple> CreateVectorEditor(ObservableCollection<string> ambiguities)
            => new AmbiguousUIntVector3DEditorViewModel(ambiguities);

        protected override Layer3D EvaluateOutput(NonHeadLayer<Layer3D, Sensor3D> prev, Ambiguous3DTuple kernel, Ambiguous3DTuple strides, PoolingType pooling)
            => Layer3D.NewPooling3D(new Pooling<Ambiguous3DTuple>(kernel, strides, pooling), prev);

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD3(HeadOutput.CurrentValue));
    }
}
