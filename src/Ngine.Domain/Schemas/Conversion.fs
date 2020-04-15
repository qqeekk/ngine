namespace Ngine.Domain.Schemas

[<AutoOpen>]
module Errors =
    type InvalidActivatorSyntaxMessage = Message of string
    type PatternPropertyName = PatternProperty of string

    type InvalidActivatorSyntaxInfo = {
        Position : uint32 * uint32
        Message : InvalidActivatorSyntaxMessage
    }

    type PatternMissmatchInfo = {
        Pattern: string
        PropertyNames: PatternPropertyName[]
    }

    type ValueOutOfRangeInfo = {
        Property: PatternPropertyName
        IndicatedValue: string
    }

    type PropsConversionError =
        | ValuesOutOfRange of ValueOutOfRangeInfo []
        | PropsPatternMissmatch of PatternMissmatchInfo
        | InvalidActivatorSyntax of InvalidActivatorSyntaxInfo[]

    type LayerConversionError =
        | UnknownType of string
        | ExpectedLayerId
        | MissingLayerId of Schema.LayerId
        | PropsConversionError of PropsConversionError
        | PrevLayerPropsEmpty

    type LayerCompatibilityError =
        | DimensionMissmatch
        | DuplicateLayerId

    type LayerCompatibilityErrorInfo = {
        Layer2: Schema.Layer
        Error: LayerCompatibilityError
    }

    type LayerError =
        | LayerCompatibilityError of LayerCompatibilityErrorInfo
        | LayerError of LayerConversionError[]
        | AggregateLayerError of (Schema.Layer * LayerError)[]

    type LossError =
        | UnknownType

    type HeadError =
        | LayerError of Schema.Layer option * LayerError
        | LossError of LossError
        | HeadFunctionError of InvalidActivatorSyntaxInfo

    type NetworkConversionError =
        | LayerError of Schema.Layer * LayerError
        | OptimizerError of InvalidActivatorSyntaxInfo
        | HeadError of Schema.Head * HeadError[]

type Sensor =
    | Sensor3D of Sensor3D
    | Sensor2D of Sensor2D
    | Sensor1D of Sensor1D

type LayerProps =
    | Dense of Dense
    | Sensor of Sensor
    | Convolutional2D of Convolutional2D
    | Convolutional3D of Convolutional3D
    | Pooling2D of Pooling2D
    | Pooling3D of Pooling3D
    | Activator of Activator
    | PrevLayers of Schema.LayerId []
    | Flatten3D
    | Flatten2D
    | Dropout of float

type HeadFunction =
    | Softmax
    | Activator of Activator

type LayerPropsDecoder = delegate of schema:string -> Result<LayerProps, PropsConversionError>

[<Interface>]
type ILayerPropsConverter =
    abstract member Encode: LayerProps -> string
    abstract member EncodeLayerType: LayerProps -> string
    abstract member EncodeLoss: Loss -> string
    abstract member EncodeHeadActivation: HeadFunction -> string
    abstract member EncodeOptimizer: Optimizer -> string

    abstract member Decode: layerType:string -> LayerPropsDecoder option
    abstract member DecodeLoss: string -> Result<Loss, LossError>
    abstract member DecodeHeadActivation: string -> Result<HeadFunction, InvalidActivatorSyntaxInfo>
    abstract member DecodeOptimizer: string -> Result<Optimizer, InvalidActivatorSyntaxInfo>

    abstract member LayerTypeNames : string [] with get

[<Interface>]
type IActivatorConverter =
    abstract member Encode: Activator-> string
    abstract member Decode: string -> Result<Activator, InvalidActivatorSyntaxInfo[]>
    abstract member QuotedFunctionNames : string [] with get

[<Interface>]
type INetworkConverter =
    abstract member Encode: Network -> Schema.Network
    abstract member Decode: Schema.Network -> Result<Network, NetworkConversionError[]>
