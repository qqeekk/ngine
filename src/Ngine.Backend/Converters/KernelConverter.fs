namespace Ngine.Backend.Converters
open Ngine.Domain.Utils
open Ngine.Domain.Schemas
open FSharp.Text.RegexProvider
open FSharp.Text.RegexExtensions
open Ngine.Backend.Resources.Properties
open Keras
open System

[<RequireQualifiedAccess>]
[<Struct>]
type private KernelTypeNames =
    | Conv2D
    | Conv3D
    | Dense

module private Encoders =
    type private IntNumericGroupRegex = Regex< @"\(\?\<(?<group>[a-zA-Z]+)\>\\d\+\)" >
    let inline private encode regexPattern regexObject mapping =
        let properties : Map<_,_> =
            lazy (^a : (member TypedMatch : string -> ^b) (regexObject, "")) |> mapping

        IntNumericGroupRegex().TypedReplace(regexPattern, fun m -> properties.[m.group.Value])

    let inline private decode regexPattern regexObject mapping =
        new LayerPropsDecoder (fun schema ->
            match (^a : (member TryTypedMatch : string -> ^b option) (regexObject, schema)) with
            | Some m -> mapping m
            | None ->
                let properties = (regexObject :> System.Text.RegularExpressions.Regex).GetGroupNames()
                Error (PropsPatternMissmatch {
                    PropertyNames = Array.map (PatternProperty) properties
                    Pattern = encode regexPattern regexObject (fun _ ->
                        properties
                        |> Seq.map (fun group -> (group, sprintf "${{ %s }}" group))
                        |> Map.ofSeq)
                    |> Regex.Unescape })
        )

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

module public KernelConverter = 
    let private tryParseNumber (num : System.Text.RegularExpressions.Group) =
        match num.TryAsUInt32 with
        | Some n -> Ok n
        | None -> Error { 
            Property = PatternProperty num.Name
            IndicatedValue = num.Value }

    let private convertRawValuesToLayer conversion =
        Seq.map tryParseNumber
        >> ResultExtensions.aggregate
        >> function
        | Ok v -> Ok (conversion v)
        | Error messages -> Error (ValuesOutOfRange messages)

    let private mappings = BijectiveMap [|
        (Recources.Kernels_conv3D, KernelTypeNames.Conv3D)
        (Recources.Kernels_conv2D, KernelTypeNames.Conv2D)
        (Recources.Kernels_dense, KernelTypeNames.Dense)
    |]

    let private encodeKernelType (NotNull "kernel" kernel) =
        let kernelLabel =
            match kernel with
            | Conv3D _ -> KernelTypeNames.Conv3D
            | Conv2D _ -> KernelTypeNames.Conv2D
            | Dense -> KernelTypeNames.Conv2D

        mappings.TryGetLeft kernelLabel |> Option.get

    let private encodeKernel total (NotNull "kernel" kernel) = 
        match kernel with
        | Conv3D (NotNull "kernel layout" c3d) -> Encoders.Conv3D.encode <| fun m -> Map.ofList [
            (nameof(m.Value.total), string total)
            (nameof(m.Value.width), string c3d.Width)
            (nameof(m.Value.height), string c3d.Height)
            (nameof(m.Value.depth), string c3d.Depth) ]

        | Conv2D (NotNull "kernel layout" c2d) -> Encoders.Conv2D.encode <| fun m -> Map.ofList [
            (nameof(m.Value.total), string total)
            (nameof(m.Value.width), string c2d.Width)
            (nameof(m.Value.height), string c2d.Height) ]

        | Dense -> Encoders.Dense.encode <| fun m -> Map.ofList [
            nameof(m.Value.total), string total ]

    let private decode (NotNull "layer type" layerType) =
        mappings.TryGetRight layerType
        |> Option.map (function
            | KernelTypeNames.Conv3D -> Encoders.Conv3D.decode <| fun m ->
                [ m.total; m.width; m.height; m.depth ]
                |> convertRawValuesToLayer (fun v -> v.[0], Conv3D { Width = v.[1]; Height = v.[2]; Depth = v.[3] })

            | KernelTypeNames.Conv2D -> Encoders.Conv2D.decode <| fun m ->
                [ m.total; m.width; m.height ]
                |> convertRawValuesToLayer (fun v -> v.[0], Conv2D { Width = v.[1]; Height = v.[2] })

            | KernelTypeNames.Dense -> Encoders.Dense.decode <| fun m ->
                match tryParseNumber m.total with
                | Ok total -> Ok (total, Dense)
                | Error message -> Error (ValuesOutOfRange [| message |])
        )

    let instance = { 
        new ILayerPropsConverter with
            member _.EncodeLayerType kernel = encodeKernelType kernel
            member _.Encode total kernel = encodeKernel total kernel
            member _.Decode layerType = decode layerType
            member _.LayerTypeNames with get() = mappings |> Seq.map (fst) |> Seq.toArray }

    let internal keras (layer : Layer) =
        let activator = ActivatorConverter.keras (layer.Activator)

        match layer.Kernel with
        | Conv2D conv2d ->
            new Layers.Conv2D(
                int layer.Filters,
                (int conv2d.Width, int conv2d.Height),
                activation = activator) :> Layers.BaseLayer

        | Conv3D conv3d ->
            new Layers.Conv3D(
                int layer.Filters,
                (int conv3d.Width, int conv3d.Height, int conv3d.Depth),
                activation = activator) :> Layers.BaseLayer

        | Dense ->
            new Layers.Dense(
                int layer.Filters,
                input_dim = Nullable 1,
                activation = activator) :> Layers.BaseLayer
