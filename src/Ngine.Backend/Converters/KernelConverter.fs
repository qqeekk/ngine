namespace Ngine.Backend.Converters
open System
open Ngine.Domain.Utils
open Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Errors
open Ngine.Backend.Resources.Properties
open Keras
open System.Text.RegularExpressions
open System.Collections.Generic
open Keras.Layers
open Ngine.Domain
open Ngine.Domain.Services.Conversion
open Python.Runtime
open System.Dynamic
open FSharp.Interop.Dynamic

[<ReferenceEquality>]
type Encoder = {
    pretty: Pretty
    decode: LayerPropsDecoder
}

module DenseEncoder =
    let private m = {| units = AmbiguousEncoder.encoder IntegerEncoder.encoder |}

    let private stringify (p:Printer) =
        p.[nameof m.units]

    let private regex =
        (seq {
            nameof m.units, m.units.regex }, None)
        ||> eval (stringify >> regComplete)

    let private pretty =
        seq {
            nameof m.units, m.units.pretty }
        |> pretty Recources.Kernels_dense regex (stringify >> Regex.Unescape)

    let private decode =
        tryDecodeByRegex regex <| fun groups ->
            m.units.decode groups 0u (nameof m.units)
            |> function
            | Ok units -> Ok (units |> Option.map (fun units -> LayerProps.Dense { Units = units }))
            | Error errors -> Error (ValuesOutOfRange (List.toArray errors))
        >> mapToError pretty

    let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }
    let encode (dense: Schemas.Dense) =
        (seq { nameof m.units, fun _ -> m.units.encode dense.Units }, None)
        ||> eval (stringify >> Regex.Unescape)

module ConvEncoder =
    type private M<'a> = {
        filters: PrimitiveEncoder<Ambiguous<uint32>, ValueOutOfRangeInfo list>
        kernel : PrimitiveEncoder<'a, ValueOutOfRangeInfo list>
        strides : PrimitiveEncoder<'a, ValueOutOfRangeInfo list>
        padding : PrimitiveEncoder<Padding, ValueOutOfRangeInfo>
    }

    let private m encoder = {
        filters = AmbiguousEncoder.encoder IntegerEncoder.encoder
        kernel = encoder
        strides = encoder
        padding = PaddingEncoder.encoder }

    let private stringify mx createOptional (p:Printer) =
        let f = p.[nameof mx.filters]
        let k = p.[nameof mx.kernel]
        let s = p.[nameof mx.strides]
        let p = p.[nameof mx.padding]

        sprintf "%s:%s" f k
        + createOptional (", strides=" + s)
        + createOptional (", padding=" + p)

    let private mkRegex mx =
        (seq {
            nameof mx.filters, mx.filters.regex
            nameof mx.kernel, mx.kernel.regex
            nameof mx.strides, mx.strides.regex
            nameof mx.padding, mx.padding.regex }, None)
        ||> eval (stringify mx regOptional >> regComplete)

    let private mkPretty mx prettyname =
        seq {
            nameof mx.filters, mx.filters.pretty
            nameof mx.kernel, mx.kernel.pretty
            nameof mx.strides, mx.strides.pretty
            nameof mx.padding, mx.padding.pretty }
        |> pretty prettyname (mkRegex mx) (stringify mx regOptional)

    let private decode mx pretty create =
        tryDecodeByRegex (pretty.regex) <| fun groups ->
            let kernel = mx.kernel.decode groups 0u (nameof mx.kernel)
            let strides = mx.strides.decode groups 0u (nameof mx.strides)
            let padding = mx.padding.decode groups 0u (nameof mx.padding) |> Result.mapError (List.singleton)
            let filters = mx.filters.decode groups 0u (nameof mx.filters)

            match ResultExtensions.zip4 filters kernel strides padding with
            | Ok (Some filters, Some kernel, strides, padding) ->
                Ok (Some <| create filters kernel strides padding)
            | Ok _ -> Ok None
            | Error errors -> Error (ValuesOutOfRange (Seq.concat errors |> Seq.toArray))
        >> mapToError pretty

    let private encode m mappings =
        eval (stringify m id >> Regex.Unescape) mappings None

    module D2 =
        let private m2 = m (Vector2DEncoder.encoder <| AmbiguousEncoder.encoder IntegerEncoder.encoder)
        let private pretty = mkPretty m2 Recources.Kernels_conv2D

        let private decode =
            decode m2 pretty <| fun f k s p -> Convolutional2D {
                Filters = f
                Kernel = k
                Strides = Option.defaultValue (Vector2D (Fixed 1u, Fixed 1u)) s
                Padding = Option.defaultValue (Zero) p }

        let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }
        let encode (conv:Convolutional<Vector2D<Ambiguous<uint32>>>) =
            seq {
                nameof m2.filters, fun _ -> m2.filters.encode conv.Filters
                nameof m2.kernel, fun _ -> m2.kernel.encode conv.Kernel
                nameof m2.strides, fun _ -> m2.strides.encode conv.Strides
                nameof m2.padding, fun _ -> m2.padding.encode conv.Padding }
            |> encode m2

    module D3 =
        let private m3 = m (Vector3DEncoder.encoder <| AmbiguousEncoder.encoder IntegerEncoder.encoder)
        let private pretty = mkPretty m3 Recources.Kernels_conv3D

        let private decode =
            decode m3 pretty <| fun f k s p -> Convolutional3D {
                Filters = f
                Kernel = k
                Strides = Option.defaultValue (Vector3D (Fixed 1u, Fixed 1u, Fixed 1u)) s
                Padding = Option.defaultValue (Zero) p }

        let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }
        let encode (conv:Convolutional<Vector3D<Ambiguous<uint32>>>) =
            seq {
                nameof m3.filters, fun _ -> m3.filters.encode conv.Filters
                nameof m3.kernel, fun _ -> m3.kernel.encode conv.Kernel
                nameof m3.strides, fun _ -> m3.strides.encode conv.Strides
                nameof m3.padding, fun _ -> m3.padding.encode conv.Padding }
            |> encode m3

module SensorEncoder =
    type private M<'a> = {
        channels: PrimitiveEncoder<uint32, ValueOutOfRangeInfo>
        inputs : PrimitiveEncoder<'a, ValueOutOfRangeInfo list> }

    let private m encoder = {
        channels = IntegerEncoder.encoder
        inputs = encoder }

    let private stringify mx (p:Printer) =
        let c = p.[nameof mx.channels]
        let i = p.[nameof mx.inputs]

        sprintf "%s:%s" c i

    let private mkRegex mx =
        (seq {
            nameof mx.channels, mx.channels.regex
            nameof mx.inputs, mx.inputs.regex }, None)
        ||> eval (stringify mx >> regComplete)

    let private mkPretty mx prettyname =
        seq {
            nameof mx.channels, mx.channels.pretty
            nameof mx.inputs, mx.inputs.pretty }
        |> pretty prettyname (mkRegex mx) (stringify mx)

    let private encode m mappings =
        eval (stringify m >> Regex.Unescape) mappings None

    let private decode mx pretty create =
        tryDecodeByRegex (pretty.regex) <| fun groups ->
            let channels = mx.channels.decode groups 0u (nameof mx.channels) |> Result.mapError (List.singleton)
            let inputs = mx.inputs.decode groups 0u (nameof mx.inputs)

            match ResultExtensions.zip channels inputs with
            | Ok (Some channels, Some inputs) ->
                Ok (Some <| create channels inputs)
            | Ok _ -> Ok None
            | Error errors -> Error (ValuesOutOfRange (Seq.concat errors |> Seq.toArray))
        >> mapToError (pretty)

    module D2 =
        let private m2 = m (Vector2DEncoder.encoder { 
            decode = fun groups num -> IntegerEncoder.encoder.decode groups num >> Result.mapError (List.singleton)
            encode = IntegerEncoder.encoder.encode
            pretty = IntegerEncoder.encoder.pretty
            regex = IntegerEncoder.encoder.regex })

        let private pretty = mkPretty m2 Recources.Kernels_sensor2D

        let private decode =
            decode m2 pretty <| fun c i ->
                LayerProps.Sensor2D {
                    Channels = c
                    Inputs = i }

        let encode (sensor:Sensor2D) =
            seq {
                nameof m2.channels, fun _ -> m2.channels.encode sensor.Channels
                nameof m2.inputs, fun _ -> m2.inputs.encode sensor.Inputs }
            |> encode m2

        let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }

    module D3 =
        let private m3 = m (Vector3DEncoder.encoder { 
            decode = fun groups num -> IntegerEncoder.encoder.decode groups num >> Result.mapError (List.singleton)
            encode = IntegerEncoder.encoder.encode
            pretty = IntegerEncoder.encoder.pretty
            regex = IntegerEncoder.encoder.regex })

        let private pretty = mkPretty m3 Recources.Kernels_sensor3D

        let private decode =
            decode m3 pretty <| fun c i ->
                LayerProps.Sensor3D {
                    Channels = c
                    Inputs = i }

        let encode (sensor:Sensor3D) =
            seq {
                nameof m3.channels, fun _ -> m3.channels.encode sensor.Channels
                nameof m3.inputs, fun _ -> m3.inputs.encode sensor.Inputs }
            |> encode m3

        let encoder = {  pretty = pretty; decode = LayerPropsDecoder decode }

    module D1 =
        let private m = {|
            inputs = IntegerEncoder.encoder |}

        let private stringify (p:Printer) =
            let i = p.[nameof m.inputs]

            sprintf "%s" i

        let private regex =
            (seq {
                nameof m.inputs, m.inputs.regex }, None)
            ||> eval (stringify >> regComplete)

        let private pretty =
            seq {
                nameof m.inputs, m.inputs.pretty }
            |> pretty Recources.Kernels_sensor1D regex (stringify)

        let private decode =
            tryDecodeByRegex regex <| fun groups ->
                let inputs = m.inputs.decode groups 0u (nameof m.inputs)

                match inputs with
                | Ok (Some inputs) ->
                    Ok (Some <| LayerProps.Sensor1D { Inputs = inputs })
                | Ok _ -> Ok None
                | Error error -> Error (ValuesOutOfRange [| error |])
            >> mapToError pretty

        let encode (sensor:Sensor1D) =
            (seq {
                nameof m.inputs, fun _ -> m.inputs.encode sensor.Inputs }, None)
            ||> eval stringify

        let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }

module PoolingEncoder =
    type private M<'a> = {
        pooling: PrimitiveEncoder<PoolingType, ValueOutOfRangeInfo>
        strides: PrimitiveEncoder<'a, ValueOutOfRangeInfo list>
        kernel: PrimitiveEncoder<'a, ValueOutOfRangeInfo list> }

    let private m encoder = {
        pooling = PoolingTypeEncoder.encoder
        strides = encoder
        kernel = encoder }

    let private stringify mx createOptional (p:Printer) =
        let k = p.[nameof mx.kernel]
        let s = p.[nameof mx.strides]
        let p = p.[nameof mx.pooling]

        sprintf "%s:%s" p k
        + createOptional (", strides=" + s)

    let private mkRegex mx =
        (seq {
            nameof mx.pooling, mx.pooling.regex
            nameof mx.kernel, mx.kernel.regex
            nameof mx.strides, mx.strides.regex }, None)
        ||> eval (stringify mx regOptional >> regComplete)

    let private mkPretty mx prettyname =
        seq {
            nameof mx.pooling, mx.pooling.pretty
            nameof mx.kernel, mx.kernel.pretty
            nameof mx.strides, mx.strides.pretty }
        |> pretty prettyname (mkRegex mx) (stringify mx regOptional >> Regex.Unescape)

    let private encode mx mappings =
        eval (stringify mx id >> Regex.Unescape) mappings None

    let private decode mx pretty create =
        tryDecodeByRegex (pretty.regex) <| fun groups ->
            let kernel = mx.kernel.decode groups 0u (nameof mx.kernel)
            let strides = mx.strides.decode groups 0u (nameof mx.strides)
            let pooling = mx.pooling.decode groups 0u (nameof mx.pooling) |> Result.mapError (List.singleton)

            match ResultExtensions.zip3 pooling kernel strides with
            | Ok (Some pooling, Some kernel, strides) ->
                Ok (Some <| create pooling kernel strides)
            | Ok _ -> Ok None
            | Error errors -> Error (ValuesOutOfRange (Seq.concat errors |> Seq.toArray))
        >> mapToError pretty

    module D2 =
        let private m2 = m (Vector2DEncoder.encoder <| AmbiguousEncoder.encoder IntegerEncoder.encoder)
        let private pretty = mkPretty m2 Recources.Kernels_pooling2D

        let private decode =
            decode m2 pretty <| fun p k s -> LayerProps.Pooling2D {
                Kernel = k
                Strides = Option.defaultValue (Vector2D (Fixed 1u, Fixed 1u)) s
                PoolingType = p }

        let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }
        let encode (pooling:Pooling<Vector2D<Ambiguous<uint32>>>) =
            seq {
                nameof m2.pooling, fun _ ->  m2.pooling.encode pooling.PoolingType
                nameof m2.strides, fun _ ->  m2.strides.encode pooling.Strides
                nameof m2.kernel, fun _ -> m2.kernel.encode pooling.Kernel }
            |> encode m2

    module D3 =
        let private m3 = m (Vector3DEncoder.encoder <| AmbiguousEncoder.encoder IntegerEncoder.encoder)
        let private pretty = mkPretty m3 Recources.Kernels_pooling3D

        let private decode =
            decode m3 pretty <| fun p k s -> LayerProps.Pooling3D {
                Kernel = k
                Strides = Option.defaultValue (Vector3D (Fixed 1u, Fixed 1u, Fixed 1u)) s
                PoolingType = p }

        let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }
        let encode (pooling:Pooling<Vector3D<Ambiguous<uint32>>>) =
            seq {
                nameof m3.pooling, fun _ ->  m3.pooling.encode pooling.PoolingType
                nameof m3.strides, fun _ ->  m3.strides.encode pooling.Strides
                nameof m3.kernel, fun _ -> m3.kernel.encode pooling.Kernel }
            |> encode m3

module ConcatenationEncoder =
    let private m = {| layers = CommaSeparatedValuesEncoder.encoder LayerIdEncoder.encoder |}

    let private stringify (p:Printer) =
        p.[nameof m.layers]

    let private regex =
        (seq {
            nameof m.layers, m.layers.regex }, None)
        ||> eval (stringify >> regComplete)

    let private pretty =
        seq {
            nameof m.layers, m.layers.pretty }
        |> pretty Recources.Kernels_concatenation regex (stringify >> Regex.Unescape)

    let private decode =
        tryDecodeByRegex regex <| fun groups ->
            m.layers.decode groups 0u (nameof m.layers)
            |> function
            | Ok ids -> Ok (ids |> Option.map (fun ids -> LayerProps.PrevLayers ids))
            | Error errors -> Error (ValuesOutOfRange (Seq.concat errors |> Seq.toArray))
        >> mapToError pretty

    let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }
    let encode (ids: LayerId []) =
        (seq { nameof m.layers, fun _ -> m.layers.encode ids }, None)
        ||> eval (stringify >> Regex.Unescape)

module DropoutEncoder =
    let private m = {| rate = FloatEncoder.encoder |}

    let private stringify (p:Printer) =
        p.[nameof m.rate]

    let private regex =
        (seq {
            nameof m.rate, m.rate.regex }, None)
        ||> eval (stringify >> regComplete)

    let private pretty =
        seq {
            nameof m.rate, m.rate.pretty }
        |> pretty Recources.Kernels_dropout regex (stringify >> Regex.Unescape)

    let private decode =
        tryDecodeByRegex regex <| fun groups ->
            m.rate.decode groups 0u (nameof m.rate)
            |> function
            | Ok rate -> Ok (rate |> Option.map (fun rate -> LayerProps.Dropout rate))
            | Error error -> Error (ValuesOutOfRange [| error |])
        >> mapToError pretty

    let encode (rate: float32) =
        (seq { nameof m.rate, fun _ -> m.rate.encode rate }, None)
        ||> eval (stringify >> Regex.Unescape)

    let encoder = { pretty = pretty; decode = LayerPropsDecoder decode }

module FlattenEncoder =
    let encoder2D = {
        pretty = { name = Recources.Kernels_flatten2D; regex = ""; defn = Some "(empty)"; deps = []}
        decode = LayerPropsDecoder(fun _ -> Ok LayerProps.Flatten2D)
    }

    let encoder3D = {
        pretty = { name = Recources.Kernels_flatten3D; regex = ""; defn = Some "(empty)"; deps = []}
        decode = LayerPropsDecoder(fun _ -> Ok LayerProps.Flatten3D)
    }

    let encoded = ""

module ActivationEncoder =
    let encoder (activationEncoder: IActivatorConverter) = 
        let typeNames = activationEncoder.ActivationFunctionNames |> Array.map (fun a -> a.name)
        
        let createEncoder propName mapping = {
            pretty = {
                name = propName
                regex = "(other)"
                defn = Some <| prettyPrint (sprintf "(%s)" (String.Join('|', typeNames))) propName
                deps = activationEncoder.ActivationFunctionNames |> Array.toList }
            decode = LayerPropsDecoder(fun func ->
                activationEncoder.Decode func
                |> Result.map mapping) }
        
        {|
            encoder1D = createEncoder "activation1D" LayerProps.Activator1D
            encoder2D = createEncoder "activation2D" LayerProps.Activator2D
            encoder3D = createEncoder "activation3D" LayerProps.Activator3D |}

module KernelConverter =
    let create (activationEncoder: IActivatorConverter) =
        let activatorEncoders = ActivationEncoder.encoder activationEncoder
        
        let mappings = [|
            ConvEncoder.D3.encoder
            ConvEncoder.D2.encoder
            DenseEncoder.encoder
            PoolingEncoder.D3.encoder
            PoolingEncoder.D2.encoder
            FlattenEncoder.encoder2D
            FlattenEncoder.encoder3D
            SensorEncoder.D3.encoder
            SensorEncoder.D2.encoder
            SensorEncoder.D1.encoder
            DropoutEncoder.encoder
            ConcatenationEncoder.encoder
            activatorEncoders.encoder1D
            activatorEncoders.encoder2D
            activatorEncoders.encoder3D
        |]

        let decode (NotNull "layer type" layerType) : LayerPropsDecoder option =
            mappings |> Seq.tryPick (fun encoder ->
                if encoder.pretty.name = layerType
                then Some encoder.decode
                else None)

        let encode = function
            | LayerProps.Activator1D a
            | LayerProps.Activator2D a
            | LayerProps.Activator3D a -> activationEncoder.Encode a
            | LayerProps.PrevLayers ids -> ConcatenationEncoder.encode ids
            | LayerProps.Convolutional2D conv -> ConvEncoder.D2.encode conv
            | LayerProps.Convolutional3D conv -> ConvEncoder.D3.encode conv
            | LayerProps.Dense dense -> DenseEncoder.encode dense
            | LayerProps.Dropout d -> DropoutEncoder.encode d
            | LayerProps.Flatten2D | LayerProps.Flatten3D -> FlattenEncoder.encoded
            | LayerProps.Pooling2D p -> PoolingEncoder.D2.encode p
            | LayerProps.Pooling3D p -> PoolingEncoder.D3.encode p
            | LayerProps.Sensor3D s -> SensorEncoder.D3.encode s
            | LayerProps.Sensor2D s -> SensorEncoder.D2.encode s
            | LayerProps.Sensor1D s -> SensorEncoder.D1.encode s

        let encodeLayerType = function
            | LayerProps.Activator1D _ -> (ActivationEncoder.encoder activationEncoder).encoder1D.pretty.name
            | LayerProps.Activator2D _ -> (ActivationEncoder.encoder activationEncoder).encoder2D.pretty.name
            | LayerProps.Activator3D _ -> (ActivationEncoder.encoder activationEncoder).encoder3D.pretty.name
            | LayerProps.PrevLayers _ -> ConcatenationEncoder.encoder.pretty.name
            | LayerProps.Convolutional2D _ -> ConvEncoder.D2.encoder.pretty.name
            | LayerProps.Convolutional3D _ -> ConvEncoder.D3.encoder.pretty.name
            | LayerProps.Dense _ -> DenseEncoder.encoder.pretty.name
            | LayerProps.Dropout _ -> DropoutEncoder.encoder.pretty.name
            | LayerProps.Flatten2D -> FlattenEncoder.encoder2D.pretty.name
            | LayerProps.Flatten3D -> FlattenEncoder.encoder3D.pretty.name
            | LayerProps.Pooling2D _ -> PoolingEncoder.D2.encoder.pretty.name
            | LayerProps.Pooling3D _ -> PoolingEncoder.D3.encoder.pretty.name
            | LayerProps.Sensor3D _ -> SensorEncoder.D3.encoder.pretty.name
            | LayerProps.Sensor2D _ -> SensorEncoder.D2.encoder.pretty.name
            | LayerProps.Sensor1D _ -> SensorEncoder.D1.encoder.pretty.name

        { new ILayerPropsConverter with
            member _.EncodeLayerType kernel = encodeLayerType kernel
            member _.Encode props = encode props
            member _.Decode layerType = decode layerType
            member _.ActivatorConverter with get() = activationEncoder
            member _.LayerTypeNames with get() = mappings |> Seq.map (fun encoder -> encoder.pretty) |> Seq.toArray }

    let private kerasPadding = function
        | Zero -> "valid"
        | Same -> "same"

    let private renameLayer name (layer:BaseLayer) =
        layer.ToPython()?_name <- (LayerIdEncoder.encoder.encode name).ToPython()
        layer

    let private append (input:BaseLayer) (layer:BaseLayer) =
        new BaseLayer(layer.InvokeMethod("__call__", Dictionary(dict ["inputs", box input])))

    let rec keras layer tryGetLayer (inputs: BaseLayer list) (ambiguities:IDictionary<AmbiguityVariableName, _>) : BaseLayer * BaseLayer list =
        tryGetLayer layer inputs <| fun () ->
            match layer with
            | Choice1Of2 layer ->
                match layer with
                | D2 (HeadLayer (layerId, Layer2D.Conv2D (conv2d, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert2D prev) tryGetLayer inputs ambiguities

                    new Layers.Conv2D(
                        filters = int (Ambiguous.value ambiguities conv2d.Filters),
                        kernel_size = Vector2D.map (Ambiguous.value ambiguities >> int) conv2d.Kernel,
                        strides = Vector2D.map (Ambiguous.value ambiguities >> int) conv2d.Strides,
                        padding = kerasPadding conv2d.Padding)
                    |> renameLayer layerId |> append prev, inputs

                | D3 (HeadLayer (layerId, Layer3D.Conv3D (conv3d, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert3D prev) tryGetLayer inputs ambiguities
                    
                    new Layers.Conv3D(
                        filters = int (Ambiguous.value ambiguities conv3d.Filters),
                        kernel_size = Vector3D.map (Ambiguous.value ambiguities >> int) conv3d.Kernel,
                        strides = Vector3D.map (Ambiguous.value ambiguities >> int) conv3d.Strides,
                        padding = kerasPadding conv3d.Padding)
                    |> renameLayer layerId |> append prev, inputs


                | D1 (HeadLayer (layerId, Layer1D.Dense (d, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert1D prev) tryGetLayer inputs ambiguities

                    new Layers.Dense(int (Ambiguous.value ambiguities d.Units))
                    |> renameLayer layerId |> append prev, inputs

                | D3 (HeadLayer (layerId, Layer3D.Activation3D (a, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert3D prev) tryGetLayer inputs ambiguities
                    ActivatorConverter.keras a
                    |> renameLayer layerId |> append prev, inputs

                | D2 (HeadLayer (layerId, Layer2D.Activation2D (a, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert2D prev) tryGetLayer inputs ambiguities
                    ActivatorConverter.keras a
                    |> renameLayer layerId |> append prev, inputs

                | D1 (HeadLayer (layerId, Layer1D.Activation1D (a, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert1D prev) tryGetLayer inputs ambiguities
                    ActivatorConverter.keras a
                    |> renameLayer layerId |> append prev, inputs

                | D1 (HeadLayer (layerId, Layer1D.Dropout (p, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert1D prev) tryGetLayer inputs ambiguities
                    new Layers.Dropout(float p)
                    |> renameLayer layerId |> append prev, inputs

                | D1 (HeadLayer (layerId, Layer1D.Flatten3D prev)) ->
                    let prev, inputs = keras (NetworkConverters.convert3D prev) tryGetLayer inputs ambiguities
                    new Layers.Flatten()
                    |> renameLayer layerId |> append prev, inputs

                | D1 (HeadLayer (layerId, Layer1D.Flatten2D prev)) ->
                    let prev, inputs = keras (NetworkConverters.convert2D prev) tryGetLayer inputs ambiguities
                    new Layers.Flatten()
                    |> renameLayer layerId |> append prev, inputs

                | D2 (HeadLayer (layerId, Layer2D.Pooling2D ({ PoolingType = Max; Kernel = k }, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert2D prev) tryGetLayer inputs ambiguities

                    new Layers.MaxPooling2D(Vector2D.map (Ambiguous.value ambiguities >> int) k)
                    |> renameLayer layerId |> append prev, inputs

                | D2 (HeadLayer (layerId, Layer2D.Pooling2D ({ PoolingType = Avg; Kernel = k }, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert2D prev) tryGetLayer inputs ambiguities

                    new Layers.AveragePooling2D(Vector2D.map (Ambiguous.value ambiguities >> int) k)
                    |> renameLayer layerId |> append prev, inputs

                | D3 (HeadLayer (layerId, Layer3D.Pooling3D ({ PoolingType = Max; Kernel = k }, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert3D prev) tryGetLayer inputs ambiguities

                    new Layers.MaxPooling3D(Vector3D.map (Ambiguous.value ambiguities >> int) k)
                    |> renameLayer layerId |> append prev, inputs

                | D3 (HeadLayer (layerId, Layer3D.Pooling3D ({ PoolingType = Avg; Kernel = k }, prev))) ->
                    let prev, inputs = keras (NetworkConverters.convert3D prev) tryGetLayer inputs ambiguities

                    new Layers.AveragePooling3D(Vector3D.map (Ambiguous.value ambiguities >> int) k)
                    |> renameLayer layerId |> append prev, inputs

                | D3 (HeadLayer (layerId, Layer3D.Concatenation3D prevs)) ->
                    let layers, inputs = (inputs, prevs) ||> Array.mapFold (fun inputs prev ->
                        keras (NetworkConverters.convert3D prev) tryGetLayer inputs ambiguities)

                    new Layers.Concatenate(layers)
                    |> renameLayer layerId, inputs

                | D2 (HeadLayer (layerId, Layer2D.Concatenation2D prevs)) ->
                    let layers, inputs = (inputs, prevs) ||> Array.mapFold (fun inputs prev ->
                        keras (NetworkConverters.convert2D prev) tryGetLayer inputs ambiguities)

                    new Layers.Concatenate(layers)
                    |> renameLayer layerId, inputs

                | D1 (HeadLayer (layerId, Layer1D.Concatenation1D prevs)) ->
                    let layers, inputs = (inputs, prevs) ||> Array.mapFold (fun inputs prev ->
                        keras (NetworkConverters.convert1D prev) tryGetLayer inputs ambiguities)

                    new Layers.Concatenate(layers)
                    |> renameLayer layerId, inputs
            
            | Choice2Of2 sensor ->
                let s =
                    match sensor with
                    | Sensor.Sensor1D(inputId, s) ->
                        new Layers.Input(Shape (int s.Inputs), name = LayerIdEncoder.encoder.encode inputId)
                
                    | Sensor.Sensor2D(inputId, ({ Inputs = (x, y) } as s)) ->
                        new Layers.Input(Shape (int x, int y, int s.Channels), name = LayerIdEncoder.encoder.encode inputId)

                    | Sensor.Sensor3D (inputId, ({ Inputs = (x, y, z) } as s)) ->
                        new Layers.Input(Shape (int x, int y, int z, int s.Channels), name = LayerIdEncoder.encoder.encode inputId)
                    :> BaseLayer
                        
                s, s::inputs
