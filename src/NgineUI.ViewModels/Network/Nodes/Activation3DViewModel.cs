using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Activation3DViewModel : ActivationViewModelBase<Layer3D, Sensor3D>
    {
        public Activation3DViewModel(IActivatorConverter activatorConverter, LayerIdTracker idTracker, bool setId)
            : base(activatorConverter, idTracker, PortType.Layer3D, "Activation3D", setId)
        {
        }

        protected override Layer3D DefaultPrevious => Layer3D.Empty3D;

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD3(HeadOutput.CurrentValue));

        protected override Layer3D EvaluateOutput(NonHeadLayer<Layer3D, Sensor3D> prev, Activator function)
            => Layer3D.NewActivation3D(function, prev);
    }
}
