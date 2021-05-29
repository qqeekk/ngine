using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;

namespace NgineUI.ViewModels.Network.Nodes
{

    public class Concatenation1DViewModel : ConcatenationViewModelBase<Layer1D, Sensor1D> 
    {
        public Concatenation1DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer1D, setId)
        {
        }

        protected override Layer1D DefaultPrevious => Layer1D.Empty1D;

        public override FSharpChoice<Head, HeadLayer, Sensor> GetValue()
            => HeadLayerChoice(HeadLayer.NewD1(HeadOutput.CurrentValue));

        protected override Layer1D EvaluateOutput(NonHeadLayer<Layer1D, Sensor1D>[] layers)
            => Layer1D.NewConcatenation1D(layers);
    }
}
