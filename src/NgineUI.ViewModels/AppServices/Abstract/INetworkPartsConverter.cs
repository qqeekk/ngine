using Ngine.Domain.Schemas;
using Ngine.Infrastructure.AppServices;
using NgineUI.ViewModels.Network.Ambiguities;
using NodeNetwork.ViewModels;
using System.Collections.Generic;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    using Ambiguity = KeyValuePair<AmbiguityVariableName, Values<uint>>;
    public interface INetworkPartsConverter
    {
        InconsistentNetwork Encode(NetworkViewModel network, IEnumerable<Ambiguity> ambiguities, Optimizer optimizer);
        (NetworkViewModel, AmbiguityListViewModel, LayerIdTracker) Decode(InconsistentNetwork schema);
    }
}
 