using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Concatenation2DViewModel : ConcatenationViewModelBase<Layer2D, Sensor2D>
    {
        public Concatenation2DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer2D, "Concatenation2D", setId)
        {
        }

        protected override Layer2D EvaluateOutput(NonHeadLayer<Layer2D, Sensor2D>[] layers)
            => Layer2D.NewConcatenation2D(layers);
    }
}
