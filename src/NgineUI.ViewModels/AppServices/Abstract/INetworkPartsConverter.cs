using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using NgineUI.ViewModels.Network;
using NodeNetwork.ViewModels;
using static Ngine.Domain.Schemas.Errors;

namespace NgineUI.ViewModels.AppServices.Abstract
{
    public interface INetworkPartsConverter
    {
        Schema.Network Encode(NetworkPartsDto parts);
        FSharpResult<(NetworkViewModel, AmbiguitiesViewModel), NetworkConversionError<InconsistentLayerConversionError>[]> Decode(Schema.Network schema);
    }
}
