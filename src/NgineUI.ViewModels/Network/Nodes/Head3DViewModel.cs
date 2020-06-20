using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head3DViewModel : MultiDimensionalHeadViewModel<Layer3D>
    {
        public Head3DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, PortType.Layer3D)
        {
        }
        protected override Layer3D DefaultPrevious => Layer3D.Empty3D;

        protected override Head EvaluateValue(HeadLayer<Layer3D> prev, HeadFunction activator, Loss loss, float lossWeight)
        {
            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD3(prev), ((HeadFunction.Activator)activator).Item);
        }
    }
}
