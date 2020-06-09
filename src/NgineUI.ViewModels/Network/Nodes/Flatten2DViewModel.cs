using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace NgineUI.ViewModels.Network.Nodes
{
    public class Flatten2DViewModel : FlattenViewModelBase<Layer2D, Sensor2D>
    {
        public Flatten2DViewModel(LayerIdTracker idTracker, bool setId) : base(idTracker, PortType.Layer2D, "Flatten2D", setId)
        {
        }

        protected override Layer1D EvaluateOutput()
        {
            return Layer1D.NewFlatten2D(Previous.Value
                ?? NonHeadLayer<Layer2D, Sensor2D>.NewLayer(HeadLayer<Layer2D>.NewHeadLayer(Tuple.Create(0u, 0u), Layer2D.Empty2D)));
        }
    }
 }
 