namespace Ngine.Domain.Services.Conversion
open Ngine.Domain.Schemas

module public NetworkConverters =
    val create: kernelConverter : ILayerPropsConverter -> INetworkConverter
