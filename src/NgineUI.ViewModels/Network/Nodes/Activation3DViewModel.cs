using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Activation3DViewModel : ActivationViewModelBase<Layer3D, Sensor3D>
    {
        public Activation3DViewModel(IActivatorConverter activatorConverter, LayerIdTracker idTracker, bool setId)
            : base(activatorConverter, idTracker, PortType.Layer3D, "Activation3D", setId)
        {
        }

        protected override Layer3D EvaluateOutput(QuotedFunction function)
        {
            return Layer3D.NewActivation3D(
                Ngine.Domain.Schemas.Activator.NewQuotedFunction(function),
                Previous.Value ?? NonHeadLayer<Layer3D, Sensor3D>.NewLayer(HeadLayer<Layer3D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer3D.Empty3D)));
        }
    }
}
