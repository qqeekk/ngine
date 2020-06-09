namespace Ngine.Domain.Services.Conversion
open Ngine.Domain.Schemas

module public NetworkConverters =
    val convert1D: layer:NonHeadLayer<Layer1D, Sensor1D> -> Choice<HeadLayer, Sensor>
    val convert2D: layer:NonHeadLayer<Layer2D, Sensor2D> -> Choice<HeadLayer, Sensor>
    val convert3D: layer:NonHeadLayer<Layer3D, Sensor3D> -> Choice<HeadLayer, Sensor>

    val getLayerId: layer:NonHeadLayer<_, _> -> LayerId
    
    val create: propsConverter : ILayerPropsConverter ->
               lossConverter : ILossConverter ->
               optimizerConverter : IOptimizerConverter ->
               ambiguityConverter : IAmbiguityConverter -> INetworkConverter
