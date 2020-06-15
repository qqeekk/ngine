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
    using Ambiguous2DTuple = Tuple<Ambiguous<uint>, Ambiguous<uint>>;
    public class Pooling2DViewModel : PoolingViewModelBase<Ambiguous2DTuple, Layer2D, Sensor2D>
    {
        public Pooling2DViewModel(LayerIdTracker idTracker, ObservableCollection<string> ambiguities, bool setId)
            : base(idTracker, ambiguities, NodeType.Layer, PortType.Layer2D, "Pooling2D", setId)
        {
        }

        protected override ValueEditorViewModel<Ambiguous2DTuple> CreateVectorEditor(ObservableCollection<string> ambiguities)
            => new AmbiguousUIntVector2DEditorViewModel(ambiguities);

        protected override Layer2D EvaluateOutput(Ambiguous2DTuple kernel, Ambiguous2DTuple strides, PoolingType pooling)
        {
            return Layer2D.NewPooling2D(
                    new Pooling2D(kernel, strides, pooling),
                    Previous.Value ?? NonHeadLayer<Layer2D, Sensor2D>.NewLayer(
                        HeadLayer<Layer2D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer2D.Empty2D)));
        }
    }
}
