using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Activation2DViewModel : ActivationViewModelBase<Layer2D, Sensor2D>
    {
        public Activation2DViewModel(IActivatorConverter activatorConverter, LayerIdTracker idTracker, bool setId)
            : base(activatorConverter, idTracker, PortType.Layer2D, setId)
        {
        }

        protected override Layer2D DefaultPrevious => Layer2D.Empty2D;

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD2(HeadOutput.CurrentValue));

        protected override Layer2D EvaluateOutput(NonHeadLayer<Layer2D, Sensor2D> prev, Activator function)
            => Layer2D.NewActivation2D(function, prev);
    }
}
