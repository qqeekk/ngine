using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Activation1DViewModel : ActivationViewModelBase<Layer1D, Sensor1D>
    {
        public Activation1DViewModel(IActivatorConverter activatorConverter, LayerIdTracker idTracker, bool setId)
            : base(activatorConverter, idTracker, PortType.Layer1D, "Activation1D", setId)
        {
        }

        protected override Layer1D DefaultPrevious => Layer1D.Empty1D;

        protected override Layer1D EvaluateOutput(NonHeadLayer<Layer1D, Sensor1D> prev, Activator function)
            => Layer1D.NewActivation1D(function, prev);
    }
}
