using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Activation2DViewModel : ActivationViewModelBase<Layer2D, Sensor2D>
    {
        public Activation2DViewModel(IActivatorConverter activatorConverter, LayerIdTracker idTracker, bool setId)
            : base(activatorConverter, idTracker, PortType.Layer2D, "Activation2D", setId)
        {
        }

        protected override Layer2D EvaluateOutput(Ngine.Domain.Schemas.Activator function)
        {
            return Layer2D.NewActivation2D(function,
                Previous.Value ?? NonHeadLayer<Layer2D, Sensor2D>.NewLayer(HeadLayer<Layer2D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer2D.Empty2D)));
        }
    }
}
