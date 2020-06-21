using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network;
using NodeNetwork.ViewModels;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    public class NetworkPartsDto
    {
        public NetworkViewModel Nodes { get; set; }
        public Optimizer Optimizer { get; set; }
        public AmbiguitiesViewModel Ambiguities { get; set; }
    }
}
