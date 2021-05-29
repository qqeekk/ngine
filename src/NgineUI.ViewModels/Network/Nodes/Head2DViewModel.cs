using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head2DViewModel : MultiDimensionalHeadViewModel<Layer2D>
    {
        public Head2DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, PortType.Layer2D)
        {
        }

        protected override Layer2D DefaultPrevious => Layer2D.Empty2D;

        protected override Head EvaluateValue(HeadLayer<Layer2D> prev, HeadFunction activator, Loss loss, float lossWeight)
        {
            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD2(prev), ((HeadFunction.Activator)activator).Item);
        }
    }
}
