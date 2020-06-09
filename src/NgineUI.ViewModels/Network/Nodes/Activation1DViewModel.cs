using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Activation1DViewModel : ActivationViewModelBase<Layer1D, Sensor1D>
    {
        public Activation1DViewModel(IActivatorConverter activatorConverter, LayerIdTracker idTracker, bool setId)
            : base(activatorConverter, idTracker, PortType.Layer1D, "Activation1D", setId)
        {
        }

        protected override Layer1D EvaluateOutput(QuotedFunction function)
        {
            return Layer1D.NewActivation1D(
                Ngine.Domain.Schemas.Activator.NewQuotedFunction(function),
                Previous.Value ?? NonHeadLayer<Layer1D, Sensor1D>.NewLayer(HeadLayer<Layer1D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer1D.Empty1D)));
        }
    }
}
