using Ngine.Domain.Schemas;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    public interface INetworkPartsConverter
    {
        Schema.Network Encode(NetworkPartsDto parts);
    }
}
