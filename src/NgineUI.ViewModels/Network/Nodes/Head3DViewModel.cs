using Ngine.Domain.Schemas;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head3DViewModel : MultiDimensionalHeadViewModel<Layer3D>
    {
        public Head3DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, "Head3D")
        {
        }
        protected override Layer3D DefaultPrevious => Layer3D.Empty3D;

        protected override Head EvaluateValue(HeadLayer<Layer3D> prev, HeadFunction.Activator activator, Loss loss, float lossWeight)
        {
            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD3(prev), activator.Item);
        }
    }
}
