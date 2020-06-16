﻿using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Concatenation3DViewModel : ConcatenationViewModelBase<Layer3D, Sensor3D>
    {
        public Concatenation3DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer3D, "Concatenation3D", setId)
        {
        }

        protected override Layer3D DefaultPrevious => Layer3D.Empty3D;

        protected override Layer3D EvaluateOutput(NonHeadLayer<Layer3D, Sensor3D>[] layers) =>
            Layer3D.NewConcatenation3D(layers);
    }
}
