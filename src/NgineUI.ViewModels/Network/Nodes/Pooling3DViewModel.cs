using DynamicData;
using DynamicData.Binding;
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

namespace NgineUI.ViewModels.Network.Nodes
{
    using static Ngine.Domain.Schemas.Schema;
    using Ambiguous3DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>, Ambiguous<uint>>;

    public class Pooling3DViewModel : PoolingViewModelBase<Ambiguous3DTuple, Layer3D, Sensor3D>
    {
        public Pooling3DViewModel(LayerIdTracker idTracker, ObservableCollection<Ambiguity> ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer3D, "Pooling3D", setId)
        {
        }

        protected override ValueEditorViewModel<Ambiguous3DTuple> CreateVectorEditor(ObservableCollection<Ambiguity> ambiguities)
            => new AmbiguousUIntVector3DEditorViewModel(ambiguities);

        protected override Layer3D EvaluateOutput(Ambiguous3DTuple kernel, Ambiguous3DTuple strides, PoolingType pooling)
        {
            return Layer3D.NewPooling3D(
                    new Pooling3D(kernel, strides, pooling),
                    Previous.Value ?? NonHeadLayer<Layer3D, Sensor3D>.NewLayer(
                        HeadLayer<Layer3D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer3D.Empty3D)));
        }
    }
}
