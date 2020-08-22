using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network;
using NodeNetwork.ViewModels;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    public interface INetworkPartsConverter
    {
        InconsistentNetwork Encode(NetworkViewModel network, AmbiguitiesViewModel ambiguities, Optimizer optimizer);
        (NetworkViewModel, AmbiguitiesViewModel, Optimizer, LayerIdTracker) Decode(InconsistentNetwork schema);
    }
}
