namespace Ngine.Domain.Schemas

open System.Collections.Generic

[<AutoOpen>]
module Errors =
    type PatternPropertyName = string
    
    type Pretty = {
        regex: string
        name: string
        defn: string option
        deps: Pretty list }

    type PatternMissmatchInfo = {
        Pattern: Pretty }

    type ValueOutOfRangeInfo = {
        Property: PatternPropertyName
        IndicatedValue: string }

    type PropsConversionError =
        | ValuesOutOfRange of ValueOutOfRangeInfo []
        | PropsPatternMissmatch of PatternMissmatchInfo

    type InconsistentLayerConversionError =
        | UnknownType of string
        | InvalidAmbiguity of AmbiguityVariableName
        | MissingLayerId of LayerId
        | PropsConversionError of PropsConversionError

    type LayerConversionError =
        | Inconsistent of InconsistentLayerConversionError
        | ExpectedLayerId
        | PrevLayerPropsEmpty

    type LayerCompatibilityError =
        | DimensionMissmatch
        | DuplicateLayerId

    type LayerCompatibilityErrorInfo = {
        Layer2: Schema.Layer
        Error: LayerCompatibilityError }

    type LayerError<'TError> =
        | LayerCompatibilityError of LayerCompatibilityErrorInfo
        | LayerError of 'TError[]
        | AggregateLayerError of (Schema.Layer * LayerError<'TError>)[]

    type LossError =
        | UnknownType

    type HeadError<'TLayerError> =
        | LayerError of Schema.Layer option * LayerError<'TLayerError>
        | LossError of LossError
        | HeadFunctionError of PropsConversionError

    type LayerSequenceError<'TError> =
        | LayerError of Schema.Layer * LayerError<'TError>
        | AmbiguityError of Schema.Ambiguity * PropsConversionError[]

    type NetworkConversionError<'TLayerError> =
        | LayerSequenceError of LayerSequenceError<'TLayerError>
        | OptimizerError of PropsConversionError
        | HeadError of Schema.Head * HeadError<'TLayerError>[]

type LayerProps =
    | Dense of Dense
    | Sensor3D of Sensor3D
    | Sensor2D of Sensor2D
    | Sensor1D of Sensor1D
    | Convolutional2D of Convolutional<Vector2D<Ambiguous<uint32>>>
    | Convolutional3D of Convolutional<Vector3D<Ambiguous<uint32>>>
    | Pooling2D of Pooling<Vector2D<Ambiguous<uint32>>>
    | Pooling3D of Pooling<Vector3D<Ambiguous<uint32>>>
    | PrevLayers of LayerId []
    | Dropout of float32
    | Activator1D of Activator
    | Activator2D of Activator
    | Activator3D of Activator
    | Flatten3D
    | Flatten2D

type LayerPropsDecoder = delegate of schema:string -> Result<LayerProps, PropsConversionError>

[<Interface>]
type IActivatorConverter =
    abstract member EncodeHeadActivation: HeadFunction -> string
    abstract member DecodeHeadActivation: string -> Result<HeadFunction, PropsConversionError>
    abstract member Encode: Activator-> string
    abstract member Decode: string -> Result<Activator, PropsConversionError>

    abstract member ActivationFunctionNames : Pretty [] with get
    abstract member HeadFunctionNames : Pretty [] with get

[<Interface>]
type ILossConverter =
    abstract member EncodeLoss: Loss -> string
    abstract member DecodeLoss: string -> Result<Loss, LossError>
    abstract member LossFunctionNames: Pretty [] with get

[<Interface>]
type ILayerPropsConverter =
    abstract member Encode: LayerProps -> string
    abstract member EncodeLayerType: LayerProps -> string
    abstract member Decode: layerType:string -> LayerPropsDecoder option

    abstract member ActivatorConverter: IActivatorConverter with get
    abstract member LayerTypeNames: Pretty [] with get

[<Interface>]
type IOptimizerConverter =
    abstract member Encode: Optimizer-> string
    abstract member Decode: string -> Result<Optimizer, PropsConversionError>
    abstract member OptimizerNames: Pretty [] with get

type Ambiguity = KeyValuePair<AmbiguityVariableName, Values<uint32>> 

[<Interface>]
type IAmbiguityConverter =
    abstract member Encode: Ambiguity -> Schema.Ambiguity
    abstract member Decode: Schema.Ambiguity -> Result<Ambiguity, PropsConversionError[]>
    abstract member ListPattern: Pretty with get

[<Interface>]
type INetworkConverter =
    abstract member LayerConverter : ILayerPropsConverter with get
    abstract member AmbiguityConverter : IAmbiguityConverter with get
    abstract member LossConverter : ILossConverter with get

    abstract member Encode: Network -> Schema.Network
    abstract member Decode: Schema.Network -> Result<Network, NetworkConversionError<LayerConversionError>[]>
    abstract member EncodeLayers: layers: Choice<HeadLayer, Sensor>[] -> Schema.Layer[]
    abstract member EncodeHeads: heads: Head[] -> Schema.Head[]
    abstract member DecodeInconsistent: Schema.Network -> Result<InconsistentNetwork, NetworkConversionError<InconsistentLayerConversionError>[]>
    abstract member DecodeLayers:
        layers: seq<Schema.Layer> 
        -> ambiguities: seq<Schema.Ambiguity>
        -> Result<Choice<HeadLayer, Sensor>[] * IDictionary<AmbiguityVariableName, Values<uint32>>, LayerSequenceError<InconsistentLayerConversionError>[]>
