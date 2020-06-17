using DynamicData;
using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using NgineUI.ViewModels.AppServices.Abstract;
using NgineUI.ViewModels.Network;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System.Linq;
using System.Reactive;

namespace NgineUI.ViewModels.Control
{
    public class HeaderViewModel: ReactiveObject
    {
        private readonly NetworkViewModel network;
        private readonly AmbiguitiesViewModel ambiguities;
        private readonly INetworkPartsConverter partsConverter;

        public HeaderViewModel(NetworkViewModel network, AmbiguitiesViewModel ambiguities, INetworkPartsConverter partsConverter)
        {
            this.network = network;
            this.ambiguities = ambiguities;
            this.partsConverter = partsConverter;
            SaveModelCommand = ReactiveCommand.Create(SaveModel);
        }

        public ReactiveCommand<Unit, Unit> SaveModelCommand { get; set; }

        private void SaveModel()
        {
            var parts = new NetworkPartsDto
            {
                Ambiguities = ambiguities,
                Nodes = network.Nodes.Items.Cast<NgineNodeViewModel>().ToArray(),
                Optimizer = OptimizerConverter.encode(Optimizer.NewSGD(1e-4f, new SGD(0,0))),
            };
            var encoded = partsConverter.Encode(parts);
        }
    }
}
