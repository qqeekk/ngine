using Ngine.Domain.Schemas;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head2DViewModel : MultiDimensionalHeadViewModel<Layer2D>
    {
        public Head2DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, "Head2D")
        {
        }
        
        protected override Head EvaluateValue(HeadLayer<Layer2D> prev, HeadFunction.Activator activator, Loss loss, float lossWeight)
        {
            prev ??= HeadLayer<Layer2D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer2D.Empty2D);
            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD2(prev), activator.Item);
        }
    }
}
