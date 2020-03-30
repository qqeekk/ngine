namespace Ngine.Domain.Schemas.Conversions
open Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Expressions
open Ngine.Domain.Schemas.Kernels
open FSharp.Text.RegexProvider
open FSharp.Text.RegexExtensions

module private Kernels =
    module private Encoders =
        type private IntNumericGroupRegex = Regex< @"\(\?\<(?<group>[a-zA-Z]+)\>\\d\+\)" >

        let inline private encode regexPattern regexObject mapping =
            let properties : Map<_,_> =
                lazy (^a : (member TypedMatch : string -> ^b) (regexObject, "")) |> mapping

            IntNumericGroupRegex().TypedReplace(regexPattern, fun m -> properties.[m.group.Value])

        let inline private decode regexPattern regexObject mapping schema =
            match (^a : (member TryTypedMatch : string -> ^b option) (regexObject, schema)) with
            | Some m -> mapping m
            | None -> 
                let properties = (regexObject :> System.Text.RegularExpressions.Regex).GetGroupNames()
                Error (KernelPatternMissmatch { 
                    PropertyNames = Array.map (KernelProperty) properties
                    Pattern = encode regexPattern regexObject (fun _ ->
                        properties
                        |> Seq.map (fun group -> (group, sprintf "{%s}" group))
                        |> Map.ofSeq)
                    |> Regex.Unescape })

        module Conv2D =
            [<Literal>]
            let private conv2DRegexText = @"(?<total>\d+):\[(?<width>\d+)x(?<height>\d+)\]"
            type private Conv2DRegex = Regex<conv2DRegexText>
            
            let encode = Conv2DRegex () |> encode conv2DRegexText
            let decode = Conv2DRegex () |> decode conv2DRegexText

        module Conv3D =
            [<Literal>]
            let private conv3DRegexText = @"(?<total>\d+):\[(?<width>\d+)x(?<height>\d+)x(?<depth>\d+)\]"
            type private Conv3DRegex = Regex<conv3DRegexText>

            let encode = Conv3DRegex () |> encode conv3DRegexText
            let decode = Conv3DRegex () |> decode conv3DRegexText
    
        module Dense =
            [<Literal>]
            let private denseRegexText = @"(?<total>\d+)"
            type private DenseRegex = Regex<denseRegexText>

            let encode = DenseRegex () |> encode denseRegexText
            let decode = DenseRegex () |> decode denseRegexText

    let encodeLayerType = function
        | Conv3D _ -> Raw.LayerTypes.conv3D
        | Conv2D _ -> Raw.LayerTypes.conv2D
        | Dense -> Raw.LayerTypes.dense

    let encodeKernel total = function
        | Conv3D c3d -> Encoders.Conv3D.encode <| fun m -> Map.ofList [
            (nameof(m.Value.total), string total)
            (nameof(m.Value.width), string c3d.Width)
            (nameof(m.Value.height), string c3d.Height)
            (nameof(m.Value.depth), string c3d.Depth) ]

        | Conv2D c2d -> Encoders.Conv2D.encode <| fun m -> Map.ofList [
            (nameof(m.Value.total), string total)
            (nameof(m.Value.width), string c2d.Width)
            (nameof(m.Value.height), string c2d.Height) ]

        | Dense -> Encoders.Dense.encode <| fun m -> Map.ofList [
            nameof(m.Value.total), string total ]

    let decode layerType : Result<string -> Result<uint32 * Kernel, KernelConversionError>, UnknownLayerType> =
        let tryParseNumber (num : System.Text.RegularExpressions.Group) =
            match num.TryAsUInt32 with
            | Some n -> Ok n
            | None -> Error { 
                Property = KernelProperty num.Name
                IndicatedValue = num.Value }

        let convertRawValuesToLayer (conversion: _ -> uint32 * Kernel) =
            Seq.map tryParseNumber
            >> ResultExtensions.aggregateResults
            >> function
            | Ok v -> Ok (conversion v)
            | Error messages -> Error (ValuesOutOfRange messages)

        match layerType with
        | Raw.LayerTypes.conv3D -> Ok (Encoders.Conv3D.decode <| fun m ->
            [ m.total; m.width; m.height; m.depth ] 
            |> convertRawValuesToLayer (fun v -> v.[0], Conv3D { Width = v.[1]; Height = v.[2]; Depth = v.[3] }))

        | Raw.LayerTypes.conv2D -> Ok (Encoders.Conv2D.decode <| fun m ->
            [ m.total; m.width; m.height ]
            |> convertRawValuesToLayer (fun v -> v.[0], Conv2D { Width = v.[1]; Height = v.[2] }))

        | Raw.LayerTypes.dense -> Ok (Encoders.Dense.decode <| fun m ->
            match tryParseNumber m.total with
            | Ok total -> Ok (total, Dense)
            | Error message -> Error (ValuesOutOfRange [| message |]))

        | _ -> Error (UnknownLayerType layerType)


module private Activators =
    let encode activator =
        match activator with
        | QuotedFunction(Sigmoid) -> Raw.Functions.sigmoid
        | QuotedFunction(ReLu) -> Raw.Functions.relu

    let decode (schema : string) : Result<Activator, InvalidActivatorSyntaxInfo[]> = 
        match schema with
        | Raw.Functions.sigmoid -> Ok (QuotedFunction Sigmoid)
        | Raw.Functions.relu -> Ok (QuotedFunction ReLu)
        | func -> Error [|{ Position = (0u, uint32 schema.Length); Message = Message func }|]


type IActivatorConverter =
    abstract member Encode: Activator -> string
    abstract member Decode: string -> Result<Activator, InvalidActivatorSyntaxInfo[]>

type IKernelConverter =
    abstract member EncodeType: Kernel -> string
    abstract member EncodeKernel: total:uint32 -> Kernel -> string
    abstract member Decode: layerType:string -> Result<string -> Result<uint32 * Kernel, KernelConversionError>, UnknownLayerType>

module private Layers =   
    let encode (activatorConverter:IActivatorConverter) (kernelConverter:IKernelConverter) layer : Raw.Layer = 
        { Type =  kernelConverter.EncodeType (layer.Kernel)
          Neurons = kernelConverter.EncodeKernel (layer.NeuronsTotal) (layer.Kernel)
          Activator = activatorConverter.Encode (layer.Activator) }

    let decode (activatorConverter:IActivatorConverter) (kernelConverter:IKernelConverter) (schema : Raw.Layer) : Result<Layer, LayerConversionErrorInfo> =
        let neuronsResult = 
            match kernelConverter.Decode (schema.Type) with
            | Ok neuronsDecoder ->
                neuronsDecoder (schema.Neurons) 
                |> Result.mapError (KernelConversionError)

            | Error e -> Error (UnknownType e)

        let activatorResult = 
            activatorConverter.Decode (schema.Activator)
            |> Result.mapError (InvalidActivatorSyntax)
        
        match ResultExtensions.zipResults activatorResult neuronsResult with
        | Ok (activator, (total, kernel)) -> Ok { 
            Activator = activator
            NeuronsTotal = total
            Kernel = kernel }

        | Error errors -> Error { 
            Layer = schema
            Errors = List.toArray errors }

module public Networks =
    let encode (activatorConverter:IActivatorConverter) (kernelConverter:IKernelConverter) entity : Raw.Network =
        { Layers = Array.map (Layers.encode activatorConverter kernelConverter) (entity.Layers) }

    let decode (activatorConverter:IActivatorConverter) (kernelConverter:IKernelConverter) (schema : Raw.Network) : Result<Network, NetworkConversionError> = 
        schema.Layers 
        |> Seq.map (Layers.decode activatorConverter kernelConverter)
        |> ResultExtensions.aggregateResults
        |> function
        | Ok layers -> Ok { Layers = layers }
        | Error errors -> Error { LayerErrors = errors }
