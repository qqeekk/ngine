using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Ambiguities;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NgineUI.ViewModels.Network.Nodes
{
    using Ambiguous2DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>>;

    public class Conv2DViewModel : ConvViewModelBase<Ambiguous2DTuple, Layer2D, Sensor2D>
    {
        public Conv2DViewModel(LayerIdTracker idTracker, AmbiguityListViewModel ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer2D, setId)
        {
        }

        protected override Layer2D DefaultPrevious => Layer2D.Empty2D;

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD2(HeadOutput.CurrentValue));

        protected override ValueEditorViewModel<Ambiguous2DTuple> CreateVectorEditor(AmbiguityListViewModel ambiguities)
            => new AmbiguousUIntVector2DEditorViewModel(ambiguities);

        protected override Layer2D EvaluateOutput(NonHeadLayer<Layer2D, Sensor2D> prev, AmbiguousUIntViewModel filters, Ambiguous2DTuple kernel, Ambiguous2DTuple strides, Padding padding)
            => Layer2D.NewConv2D(new Convolutional<Ambiguous2DTuple>(filters, kernel, strides, padding), prev);
    }
}
