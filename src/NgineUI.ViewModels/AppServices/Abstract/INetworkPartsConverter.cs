using Ngine.Domain.Schemas;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    public interface INetworkPartsConverter
    {
        InconsistentNetwork Encode(MainViewModel parts);
        void Decode(InconsistentNetwork schema, MainViewModel viewModel);
    }
}
