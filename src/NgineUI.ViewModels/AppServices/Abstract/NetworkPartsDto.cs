using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    public class NetworkPartsDto
    {
        public NgineNodeViewModel[] Nodes { get; set; }
        public string Optimizer { get; set; }
        public AmbiguitiesViewModel Ambiguities { get; set; }
    }
}
