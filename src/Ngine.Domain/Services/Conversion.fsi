namespace Ngine.Domain.Services.Conversion
open Ngine.Domain.Schemas

module public NetworkConverters =
    val convert1D: layer:NonHeadLayer1D -> Choice<Layer, Sensor>
    val convert2D: layer:NonHeadLayer2D -> Choice<Layer, Sensor>
    val convert3D: layer:NonHeadLayer3D -> Choice<Layer, Sensor>
    
    val create: propsConverter : ILayerPropsConverter ->
               lossConverter : ILossConverter ->
               optimizerConverter : IOptimizerConverter ->
               ambiguityConverter : IAmbiguityConverter -> INetworkConverter
