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
    using Ambiguous3DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>, Ambiguous<uint>>;

    public class Conv3DViewModel : ConvViewModelBase<Ambiguous3DTuple, Layer3D, Sensor3D>
    {
        public Conv3DViewModel(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer3D, "Conv3D", setId)
        {
        }

        protected override ValueEditorViewModel<Ambiguous3DTuple> CreateVectorEditor(ObservableCollection<string> ambiguities)
            => new AmbiguousUIntVector3DEditorViewModel(ambiguities);

        protected override Layer3D EvaluateOutput(
            AmbiguousUIntViewModel filters,
            Ambiguous3DTuple kernel,
            Ambiguous3DTuple strides,
            Padding padding)
        {
            return Layer3D.NewConv3D(
                new Convolutional3D(filters, kernel, strides, padding),
                Previous.Value ?? NonHeadLayer<Layer3D, Sensor3D>.NewLayer(HeadLayer<Layer3D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer3D.Empty3D)));
        }
    }
}
