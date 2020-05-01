﻿namespace Ngine.Domain.Schemas

open System.Collections.Generic

[<AutoOpen>]
module Errors =
    type InvalidActivatorSyntaxMessage = Message of string
    type PatternPropertyName = string
    
    type Pretty = {
        regex: string
        name: string
        defn: string option
        deps: Pretty list }

    type InvalidActivatorSyntaxInfo = {
        Position : uint32 * uint32
        Message : InvalidActivatorSyntaxMessage
    }

    type PatternMissmatchInfo = {
        Pattern: Pretty
        // PropertyNames: PatternPropertyName[]
    }

    type ValueOutOfRangeInfo = {
        Property: PatternPropertyName
        IndicatedValue: string
    }

    type PropsConversionError =
        | ValuesOutOfRange of ValueOutOfRangeInfo []
        | PropsPatternMissmatch of PatternMissmatchInfo
        //| InvalidActivatorSyntax of InvalidActivatorSyntaxInfo[]

    type LayerConversionError =
        | UnknownType of string
        | InvalidAmbiguity of AmbiguityVariableName
        | ExpectedLayerId
        | MissingLayerId of LayerId
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
        | HeadFunctionError of PropsConversionError

    type NetworkConversionError =
        | LayerError of Schema.Layer * LayerError
        | OptimizerError of PropsConversionError
        | AmbiguityError of Schema.Ambiguity * PropsConversionError[]
        | HeadError of Schema.Head * HeadError[]

type Sensor =
    | Sensor3D of LayerId * Sensor3D
    | Sensor2D of LayerId * Sensor2D
    | Sensor1D of LayerId * Sensor1D

type LayerProps =
    | Dense of Dense
    | Sensor3D of Sensor3D
    | Sensor2D of Sensor2D
    | Sensor1D of Sensor1D
    | Convolutional2D of Convolutional2D
    | Convolutional3D of Convolutional3D
    | Pooling2D of Pooling2D
    | Pooling3D of Pooling3D
    | PrevLayers of LayerId []
    | Dropout of float32
    | Activator of Activator
    | Flatten3D
    | Flatten2D

type HeadFunction =
    | Softmax
    | Activator of Activator

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

[<Interface>]
type INetworkConverter =
    abstract member Encode: Network -> Schema.Network
    abstract member Decode: Schema.Network -> Result<Network, NetworkConversionError[]>

[<Interface>]
type IAmbiguityConverter =
    abstract member Encode: KeyValuePair<AmbiguityVariableName, Values<uint32>> -> Schema.Ambiguity
    abstract member Decode: Schema.Ambiguity -> Result<KeyValuePair<AmbiguityVariableName, Values<uint32>>, PropsConversionError[]>
    abstract member ListPattern: Pretty with get