using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Flatten3DViewModel : FlattenViewModelBase<Layer3D, Sensor3D>
    {
        public Flatten3DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer3D, "Flatten3D", setId)
        {
        }

        protected override Layer1D EvaluateOutput()
        {
            return Layer1D.NewFlatten3D(Previous.Value
                ?? NonHeadLayer<Layer3D, Sensor3D>.NewLayer(HeadLayer<Layer3D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer3D.Empty3D)));
        }
    }
}
