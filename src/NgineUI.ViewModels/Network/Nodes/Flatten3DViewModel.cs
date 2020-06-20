using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Flatten3DViewModel : FlattenViewModelBase<Layer3D, Sensor3D>
    {
        public Flatten3DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer3D, setId)
        {
        }

        protected override Layer3D DefaultPrevious => Layer3D.Empty3D;

        protected override Layer1D EvaluateOutput(NonHeadLayer<Layer3D, Sensor3D> prev)
            => Layer1D.NewFlatten3D(prev);
    }
}
