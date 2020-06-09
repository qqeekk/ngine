using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;

namespace NgineUI.ViewModels.Network.Nodes
{

    public class Concatenation1DViewModel : ConcatenationViewModelBase<Layer1D, Sensor1D> 
    {
        public Concatenation1DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer1D, "Concatenation1D", setId)
        {
        }

        protected override Layer1D EvaluateOutput(NonHeadLayer<Layer1D, Sensor1D>[] layers)
            => Layer1D.NewConcatenation1D(layers);
    }
}
