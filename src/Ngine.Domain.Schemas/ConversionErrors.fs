namespace Ngine.Domain.Schemas

[<AutoOpen>]
module Errors =
    type UnknownLayerType = UnknownLayerType of string
    type InvalidActivatorSyntaxMessage = Message of string
    type KernelPropertyName = KernelProperty of string

    type InvalidActivatorSyntaxInfo = {
        Position : uint32 * uint32
        Message : InvalidActivatorSyntaxMessage
    }

    type KernelPatternMissmatchInfo = {
        Pattern: string
        PropertyNames: KernelPropertyName[]
    }

    type ValueOutOfRangeInfo = {
        Property: KernelPropertyName
        IndicatedValue: string
    }

    type KernelConversionError =
        | ValuesOutOfRange of ValueOutOfRangeInfo []
        | KernelPatternMissmatch of KernelPatternMissmatchInfo

    type LayerConversionError =
        | UnknownType of UnknownLayerType
        | KernelConversionError of KernelConversionError
        | InvalidActivatorSyntax of InvalidActivatorSyntaxInfo[]
    
    type LayerConversionErrorInfo = {
        Layer: Raw.Layer
        Errors: LayerConversionError[]
    }

    type NetworkConversionError = {
        LayerErrors: LayerConversionErrorInfo[]
    }
