using Ngine.Domain.Schemas;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Head3DViewModel : MultiDimensionalHeadViewModel<Layer3D>
    {
        public Head3DViewModel(
            IActivatorConverter activatorConverter,
            ILossConverter lossConverter) : base(activatorConverter, lossConverter, "Head3D")
        {
        }

        protected override Head EvaluateValue(HeadLayer<Layer3D> prev, HeadFunction.Activator activator, Loss loss, float lossWeight)
        {
            prev ??= HeadLayer<Layer3D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer3D.Empty3D);
            return Head.NewActivator(lossWeight, loss, HeadLayer.NewD3(prev), activator.Item);
        }
    }
}
