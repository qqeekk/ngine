using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Concatenation2DViewModel : ConcatenationViewModelBase<Layer2D, Sensor2D>
    {
        public Concatenation2DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer2D, setId)
        {
        }

        protected override Layer2D DefaultPrevious => Layer2D.Empty2D;

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD2(HeadOutput.CurrentValue));

        protected override Layer2D EvaluateOutput(NonHeadLayer<Layer2D, Sensor2D>[] layers)
            => Layer2D.NewConcatenation2D(layers);
    }
}
