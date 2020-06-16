using Ngine.Domain.Schemas;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head2DViewModel : MultiDimensionalHeadViewModel<Layer2D>
    {
        public Head2DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, "Head2D")
        {
        }

        protected override Layer2D DefaultPrevious => Layer2D.Empty2D;

        protected override Head EvaluateValue(HeadLayer<Layer2D> prev, HeadFunction.Activator activator, Loss loss, float lossWeight)
        {
            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD2(prev), activator.Item);
        }
    }
}
