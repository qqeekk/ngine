using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using NgineUI.ViewModels.Network.Editors;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.ObjectModel;

namespace NgineUI.ViewModels.Network.Nodes
{
    using static Ngine.Domain.Schemas.Schema;
    using Ambiguous2DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>>;

    public class Conv2DViewModel : ConvViewModelBase<Ambiguous2DTuple, Layer2D, Sensor2D>
    {
        public Conv2DViewModel(LayerIdTracker idTracker, ObservableCollection<Ambiguity> ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer2D, "Conv2D", setId)
        {
        }

        protected override ValueEditorViewModel<Ambiguous2DTuple> CreateVectorEditor(ObservableCollection<Ambiguity> ambiguities)
            => new AmbiguousUIntVector2DEditorViewModel(ambiguities);

        protected override Layer2D EvaluateOutput(AmbiguousUIntViewModel filters, Ambiguous2DTuple kernel, Ambiguous2DTuple strides, Padding padding)
        {
            return Layer2D.NewConv2D(
                new Convolutional2D(filters, kernel, strides, padding),
                Previous.Value ?? NonHeadLayer<Layer2D, Sensor2D>.NewLayer(HeadLayer<Layer2D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer2D.Empty2D)));
        }
    }
}
