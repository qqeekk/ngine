namespace Ngine.Domain.Services.Conversion
open Ngine.Domain.Utils
open Ngine.Domain.Schemas
open System.Collections.Generic
open System

module public NetworkConverters =

    let convert3D = function
        | Layer3D (id, layer) -> Choice1Of2 (D3 (id, layer))
        | NonHeadLayer3D.Sensor3D (id, sensor) -> Choice2Of2 (Sensor.Sensor3D (id, sensor))

    let convert2D = function
        | Layer2D (id, layer) -> Choice1Of2 (D2 (id, layer))
        | NonHeadLayer2D.Sensor2D (id, sensor) -> Choice2Of2 (Sensor.Sensor2D (id, sensor))

    let convert1D = function
        | Layer1D (id, layer) -> Choice1Of2 (D1 (id, layer))
        | NonHeadLayer1D.Sensor1D (id, sensor) -> Choice2Of2 (Sensor.Sensor1D (id, sensor))

    //let validateLayerAmbiguities (ambiguities:IDictionary<AmbiguityVariableName,_>) (layers:Choice<Layer,Sensor> seq) =
    //

    //    (Ok(), layers) ||> Seq.fold (fun s -> function
    //        | Choice1Of2 (D1 (_, layer)) ->
    //            match layer with
    //            | Layer1D.Activation1D (_, _)
    //            | Layer1D.Concatenation1D _
    //            | Layer1D.Flatten2D _
    //            | Layer1D.Flatten3D _ -> Ok()
    //            | Layer1D.Dense (d, _) -> d.Units

    //    )

        //Ok ()

    let create (NotNull "props converter" propsConverter : ILayerPropsConverter)
               (NotNull "loss converter" lossConverter : ILossConverter)
               (NotNull "optimizer converter" optimizerConverter : IOptimizerConverter)
               (NotNull "ambiguity converter" ambiguityConverter : IAmbiguityConverter) =

        let encode (entity : Network) : Schema.Network =
            let map: Dictionary<Choice<Layer, Sensor>, Schema.Layer> = Dictionary()

            let ambiguities =
                entity.Ambiguities
                |> Seq.map (ambiguityConverter.Encode)
                |> Seq.toArray

            let tryGetLayerSchema layer (create: unit -> _) : _ =
                match map.TryGetValue layer with
                | true, res -> res
                | false, _ ->
                    let result = create()
                    do map.[layer] <- result
                    result

            let createSchema (layerId: LayerConnection) props : Schema.Layer =
                { LayerId = layerId
                  Type = propsConverter.EncodeLayerType props
                  Props = propsConverter.Encode props }

            let concatenate processConcatenatedLayers (layerId: LayerId) layers =
                let prevLayers : LayerId [] =
                    layers |> Array.map (processConcatenatedLayers >> fst)

                createSchema (layerId, None) (PrevLayers prevLayers)

            let rec encode (NotNull "layer" layer: Choice<Layer, Sensor>) =
                let encodeLayer prev props layerId =
                    let ({ LayerId = (prevId, _) }: Schema.Layer) = encode prev
                    createSchema (layerId, Some prevId) props

                let appendLayerToConcatenation layer =
                    let r = encode layer
                    r.LayerId

                let processConcatenatedLayers1D = appendLayerToConcatenation << convert1D
                let processConcatenatedLayers2D = appendLayerToConcatenation << convert2D
                let processConcatenatedLayers3D = appendLayerToConcatenation << convert3D
                let convert3D = encodeLayer << convert3D
                let convert2D = encodeLayer << convert2D
                let convert1D = encodeLayer << convert1D

                tryGetLayerSchema layer <| fun () ->
                    match layer with
                    | Choice1Of2 layer ->
                        match layer with
                        | D3 (layerId, Concatenation3D layers) ->
                            concatenate (processConcatenatedLayers3D) layerId layers

                        | D3 (layerId, Layer3D.Conv3D (conv, prev)) ->
                            convert3D prev (LayerProps.Convolutional3D conv) layerId

                        | D3 (layerId, Layer3D.Pooling3D (poling, prev)) ->
                            convert3D prev (LayerProps.Pooling3D poling) layerId

                        | D3 (layerId, Layer3D.Activation3D (activator, prev)) ->
                            convert3D prev (LayerProps.Activator3D activator) layerId

                        | D2 (layerId, Concatenation2D layers) ->
                            concatenate (processConcatenatedLayers2D) layerId layers

                        | D2 (layerId, Layer2D.Conv2D (conv, prev)) ->
                            convert2D prev (LayerProps.Convolutional2D conv) layerId

                        | D2 (layerId, Layer2D.Pooling2D (poling, prev)) ->
                            convert2D prev (LayerProps.Pooling2D poling) layerId

                        | D2 (layerId, Layer2D.Activation2D (activator, prev)) ->
                            convert2D prev (LayerProps.Activator2D activator) layerId

                        | D1 (layerId, Layer1D.Flatten3D layer) ->
                            convert3D layer (LayerProps.Flatten3D) layerId

                        | D1 (layerId, Layer1D.Flatten2D layer) ->
                            convert2D layer (LayerProps.Flatten2D) layerId

                        | D1 (layerId, Layer1D.Dense (dense, layer)) ->
                            convert1D layer (LayerProps.Dense dense) layerId

                        | D1 (layerId, Layer1D.Activation1D (activator, layer)) ->
                            convert1D layer (LayerProps.Activator1D activator) layerId

                        | D1 (layerId, Layer1D.Dropout (probability, prev)) ->
                            convert1D prev (LayerProps.Dropout probability) layerId

                        | D1 (layerId, Concatenation1D layers) ->
                            concatenate (processConcatenatedLayers1D) layerId layers

                    | Choice2Of2 sensor ->
                        match sensor with
                        | Sensor.Sensor3D (layerId, sensor) -> createSchema (layerId, None) (LayerProps.Sensor3D sensor)
                        | Sensor.Sensor2D (layerId, sensor) -> createSchema (layerId, None) (LayerProps.Sensor2D sensor)
                        | Sensor.Sensor1D (layerId, sensor) -> createSchema (layerId, None) (LayerProps.Sensor1D sensor)

            let heads : Schema.Head[]=
                entity.Heads
                |> Array.map (function
                    | Head.Activator (lossWeight, loss, layer, activator) -> {
                        LayerId = (encode <| Choice1Of2 layer).LayerId |> fst
                        Activation = propsConverter.ActivatorConverter.EncodeHeadActivation(HeadFunction.Activator activator)
                        Loss = lossConverter.EncodeLoss loss
                        LossWeight = lossWeight }

                    | Head.Softmax (lossWeight, loss, layerId, _) -> {
                        LayerId = layerId
                        Activation = propsConverter.ActivatorConverter.EncodeHeadActivation(HeadFunction.Softmax)
                        Loss = lossConverter.EncodeLoss loss
                        LossWeight = lossWeight
                    })

            { Layers = map.Values |> Seq.toArray
              Ambiguities = ambiguities
              Heads = heads
              Optimizer = optimizerConverter.Encode entity.Optimizer }

        let decodeLayers (layers: seq<Schema.Layer>)
                         (ambiguities: seq<Schema.Ambiguity>): Result<Choice<Layer, Sensor>[] * IDictionary<AmbiguityVariableName, Values<uint32>>, LayerSequenceError<InconsistentLayerConversionError>[]> =
            let ambiguities' =
                ambiguities
                |> Seq.map (fun amb ->
                    ambiguityConverter.Decode amb
                    |> Result.mapError (fun e -> amb, e))
                |> ResultExtensions.aggregate
                |> function
                | Ok kvps ->
                    Dictionary kvps |> Ok
                | Error errors ->
                    errors |> Array.map AmbiguityError |> Error

            let idmap: Dictionary<LayerId, Schema.Layer> = Dictionary()
            let idmapPopulatingResult =
                (Ok (), layers)
                ||> Seq.fold (fun prevResult layer ->
                    match idmap.TryGetValue (fst layer.LayerId) with
                    | true, existing ->
                        let error = layer, LayerCompatibilityError {
                            Layer2 = existing
                            Error = DuplicateLayerId }
                        match prevResult with
                        | Ok _ -> Error [error]
                        | Error prev -> Error (error::prev)

                    | false, _ ->
                        Ok (idmap.[fst layer.LayerId] <- layer))
                |> Result.mapError (Seq.map LayerError >> Seq.toArray)

            match ResultExtensions.zip ambiguities' idmapPopulatingResult with
            | Error errors -> Error (Array.concat errors)
            | Ok (ambiguities, _) ->
                let map: Dictionary<Schema.Layer, Result<Choice<Layer, Sensor>, Schema.Layer * LayerError<InconsistentLayerConversionError>>> = Dictionary()
                let tryGetLayerSchema layer (create: unit -> Result<_, _>) =
                    match map.TryGetValue layer with
                    | true, res -> res
                    | false, _ ->
                        let result = create()
                        do map.[layer] <-result
                        result

                let findLayer (id : LayerId) =
                    match idmap.TryGetValue id with
                    | true, layer -> Ok (layer)
                    | false, _ -> Error (MissingLayerId id)

                let (|RefChecked|) = function
                    | RefName ref -> if ambiguities.ContainsKey ref then Ok () else Error (InvalidAmbiguity ref)
                    | Ambiguous.Fixed _ -> Ok ()

                let processLayer3D = function
                    | Some (Choice1Of2 (D3 (layerId, layer))) -> Some (Layer3D (layerId, layer))
                    | Some (Choice2Of2 (Sensor.Sensor3D (layerId, sensor))) -> Some <| NonHeadLayer3D.Sensor3D (layerId, sensor)
                    | None -> Some <| Layer3D ((0u, 0u), Empty3D)
                    | _ -> None

                let processLayer2D = function
                    | Some (Choice1Of2 (D2 (layerId, layer))) -> Some <| Layer2D (layerId, layer)
                    | Some (Choice2Of2 (Sensor.Sensor2D (layerId, sensor))) -> Some <| NonHeadLayer2D.Sensor2D (layerId, sensor)
                    | None -> Some <| Layer2D ((0u, 0u), Empty2D)
                    | _ -> None

                let processLayer1D = function
                    | Some (Choice1Of2 (D1 (layerId, layer))) -> Some <| Layer1D (layerId, layer)
                    | Some (Choice2Of2 (Sensor.Sensor1D (layerId, sensor))) -> Some <| NonHeadLayer1D.Sensor1D (layerId, sensor)
                    | None -> Some <| Layer1D ((0u, 0u), Empty1D)
                    | _ -> None

                let rec decodeLayer (NotNull "layer schema" schema : Schema.Layer) : Result<Choice<Layer,Sensor>,_*_> =
                    let createCompatibilityError layer2 compaitibilityError =
                        schema, LayerCompatibilityError {
                            Layer2 = layer2
                            Error = compaitibilityError }

                    let decode (processLayer: Choice<Layer, Sensor> option -> _) = function
                    | Some prevLayerSchema ->
                        decodeLayer prevLayerSchema
                        |> Result.bind (fun layer ->
                            match processLayer (Some layer) with
                            | Some layer -> Ok (layer)
                            | None -> Error (createCompatibilityError prevLayerSchema DimensionMissmatch))
                    | None -> Ok (processLayer None |> Option.get)

                    let decodeConcatenation processLayer concatenate layers =
                        layers
                        |> Seq.map (
                            Result.mapError (fun error -> schema, LayerError.LayerError [| error |] )
                            >> Result.bind (Some >> decode processLayer))
                        |> ResultExtensions.aggregate
                        |> function
                        | Ok layers -> Ok (concatenate layers)
                        | Error errors -> Error (schema, AggregateLayerError errors)

                    tryGetLayerSchema schema <| fun () ->
                        let propsResult =
                            match propsConverter.Decode (schema.Type) with
                            | Some propsDecoder ->
                                propsDecoder.Invoke (schema.Props)
                                |> Result.mapError (PropsConversionError)

                            | None -> Error (InconsistentLayerConversionError.UnknownType (schema.Type))

                        let prevLayerResult =
                            match snd schema.LayerId with
                            | Some prevId -> findLayer prevId |> Result.map Some
                            | None -> Ok None

                        match ResultExtensions.zip prevLayerResult propsResult with
                        | Ok (prev, LayerProps.Activator1D activator) ->
                            prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Activation1D (activator, l))))

                        | Ok (prev, LayerProps.Activator2D activator) ->
                            prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Activation2D (activator, l))))

                        | Ok (prev, LayerProps.Activator3D activator) ->
                            prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Activation3D (activator, l))))

                        | Ok (prev, LayerProps.Pooling3D pooling) ->
                            let RefChecked x, RefChecked y, RefChecked z = pooling.Kernel
                            let RefChecked s1, RefChecked s2, RefChecked s3 = pooling.Strides

                            ResultExtensions.aggregate [x; y; z; s1; s2; s3]
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ ->
                                prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Pooling3D (pooling, l)))))

                        | Ok (prev, LayerProps.Convolutional3D conv) ->
                            let (RefChecked f) = conv.Filters
                            let RefChecked x, RefChecked y, RefChecked z = conv.Kernel
                            let RefChecked s1, RefChecked s2, RefChecked s3 = conv.Strides
                            
                            ResultExtensions.aggregate [f; x; y; z; s1; s2; s3]
                            |> Result.mapError(fun es -> (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Conv3D (conv, l)))))

                        | Ok (prev, LayerProps.Flatten3D) ->
                            prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Flatten3D l)))

                        | Ok (prev, LayerProps.Pooling2D pooling) ->
                            let RefChecked x, RefChecked y = pooling.Kernel
                            let RefChecked s1, RefChecked s2 = pooling.Strides
                            
                            ResultExtensions.aggregate [x; y; s1; s2]
                            |> Result.mapError(fun es -> (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Pooling2D (pooling, l)))))

                        | Ok (prev, LayerProps.Convolutional2D conv) ->
                            let (RefChecked f) = conv.Filters
                            let RefChecked x, RefChecked y = conv.Kernel
                            let RefChecked s1, RefChecked s2 = conv.Strides
                            
                            ResultExtensions.aggregate [f; x; y; s1; s2]
                            |> Result.mapError(fun es -> (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Conv2D (conv, l)))))

                        | Ok (prev, LayerProps.Flatten2D) ->
                            prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Flatten2D l)))

                        | Ok (prev, LayerProps.Dense props) ->
                            ``|RefChecked|`` props.Units
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError [| es |]))
                            |> Result.bind (fun _ -> prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Dense(props, l)))))

                        | Ok (prev, LayerProps.Dropout prob) ->
                            prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Dropout(prob, l))))

                        | Ok (_, LayerProps.PrevLayers ids) ->
                            match ids |> Seq.map (findLayer) |> List.ofSeq with
                            | layers ->
                                let firstValid : _ option =
                                    layers |> List.tryPick (function
                                    | Ok schema ->
                                        match decodeLayer schema with
                                        | Ok layer -> Some layer
                                        | Error _ -> None
                                    | Error _ -> None)

                                match firstValid with
                                | Some sample ->
                                    match sample with
                                    | Choice1Of2 (D3 _) | Choice2Of2 (Sensor.Sensor3D _) ->
                                        decodeConcatenation processLayer3D (fun layers -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Concatenation3D layers)) layers

                                    | Choice1Of2 (D2 _) | Choice2Of2 (Sensor.Sensor2D _) ->
                                        decodeConcatenation processLayer2D (fun layers -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Concatenation2D layers)) layers

                                    | Choice1Of2 (D1 _) | Choice2Of2 (Sensor.Sensor1D _) ->
                                        decodeConcatenation processLayer1D (fun layers -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Concatenation1D layers)) layers
                                | None ->
                                    // should return aggregate error
                                    decodeConcatenation processLayer3D (fun layers -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Concatenation3D layers)) layers

                        | Ok (_, LayerProps.Sensor3D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor3D (fst schema.LayerId, sensor))
                        | Ok (_, LayerProps.Sensor2D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor2D (fst schema.LayerId, sensor))
                        | Ok (_, LayerProps.Sensor1D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor1D (fst schema.LayerId, sensor))
                        | Error errors -> Error (schema, LayerError.LayerError (List.toArray errors))

                let rec containsEntry (searchSpace: Choice<Layer, Sensor> seq) (entry: Choice<Layer, Sensor>) : bool =
                    let rec isSubset (entry: Choice<Layer, Sensor>) (super: Choice<Layer, Sensor>) : bool =
                        if super = entry then true else 
                        match super with
                        | Choice1Of2 (D1 (_, Layer1D.Activation1D(_, prev)))
                        | Choice1Of2 (D1 (_, Layer1D.Dropout(_, prev)))
                        | Choice1Of2 (D1 (_, Layer1D.Dense(_, prev))) -> isSubset entry (convert1D prev)
                        | Choice1Of2 (D1 (_, Layer1D.Flatten2D prev))
                        | Choice1Of2 (D2 (_, Layer2D.Conv2D (_, prev)))
                        | Choice1Of2 (D2 (_, Layer2D.Pooling2D (_, prev)))
                        | Choice1Of2 (D2 (_, Layer2D.Activation2D (_, prev))) -> isSubset entry (convert2D prev)
                        | Choice1Of2 (D1 (_, Layer1D.Flatten3D prev))
                        | Choice1Of2 (D3 (_, Layer3D.Conv3D (_, prev)))
                        | Choice1Of2 (D3 (_, Layer3D.Pooling3D (_, prev)))
                        | Choice1Of2 (D3 (_, Layer3D.Activation3D (_, prev))) -> isSubset entry (convert3D prev)
                        | Choice1Of2 (D1 (_, Layer1D.Concatenation1D prevs)) -> containsEntry (Seq.map convert1D prevs) entry
                        | Choice1Of2 (D2 (_, Layer2D.Concatenation2D prevs)) -> containsEntry (Seq.map convert2D prevs) entry
                        | Choice1Of2 (D3 (_, Layer3D.Concatenation3D prevs)) -> containsEntry (Seq.map convert3D prevs) entry
                        | Choice1Of2 (D1 (_, Layer1D.Empty1D))
                        | Choice1Of2 (D2 (_, Layer2D.Empty2D))
                        | Choice1Of2 (D3 (_, Layer3D.Empty3D))
                        | Choice2Of2 _ -> false

                    Seq.exists (isSubset entry) searchSpace

                let layersResult = layers |> Seq.map decodeLayer
                match ResultExtensions.aggregate layersResult with
                | Ok layers ->
                    let _, uniques = 
                        Array.fold (fun (i, acc) curr -> 
                            let contains = 
                                containsEntry (seq {
                                    for ind = 0 to layers.Length do
                                        if i <> ind then layers.[ind] }) curr
                        
                            i+1, if contains then acc else curr::acc) (0, []) layers

                    Ok (Array.ofList uniques, upcast ambiguities)
                //Ok { Heads = heads; Optimizer = optimizer; Ambiguities = ambiguities }
                | Error errors -> Error (errors |> Seq.distinct |> Seq.map LayerSequenceError.LayerError |> Seq.toArray)


        let decode (schema: Schema.Network) =
            let ambiguities =
                schema.Ambiguities
                |> Seq.map (fun amb ->
                    ambiguityConverter.Decode amb
                    |> Result.mapError (fun e -> amb, e))
                |> ResultExtensions.aggregate
                |> function
                | Ok kvps ->
                    Dictionary kvps |> Ok
                | Error errors ->
                    errors |> Array.map AmbiguityError |> Error

            let idmap: Dictionary<LayerId, Schema.Layer> = Dictionary()
            let idmapPopulatingResult =
                (Ok (), schema.Layers)
                ||> Seq.fold (fun prevResult layer ->
                    match idmap.TryGetValue (fst layer.LayerId) with
                    | true, existing ->
                        let error : LayerSequenceError<LayerConversionError> =
                            (layer, LayerCompatibilityError {
                                Layer2 = existing
                                Error = DuplicateLayerId })
                            |> LayerError

                        match prevResult with
                        | Ok _ -> Error [error]
                        | Error prev -> Error (error::prev)

                    | false, _ ->
                        Ok (idmap.[fst layer.LayerId] <- layer))
                |> Result.mapError (Seq.toArray)

            match ResultExtensions.zip ambiguities idmapPopulatingResult with
            | Ok (ambiguities, _) ->
                let map: Dictionary<Schema.Layer, Result<Choice<Layer, Sensor>, Schema.Layer * LayerError<LayerConversionError>>> = Dictionary()
                let tryGetLayerSchema layer (create: unit -> Result<_, _>) =
                    match map.TryGetValue layer with
                    | true, res -> res
                    | false, _ ->
                        let result = create()
                        do map.[layer] <-result
                        result

                let findLayer (id : LayerId) =
                    match idmap.TryGetValue id with
                    | true, layer -> Ok (layer)
                    | false, _ -> Error (MissingLayerId id)

                let (|RefChecked|) = function
                    | RefName ref -> if ambiguities.ContainsKey ref then Ok () else Error (InvalidAmbiguity ref |> Inconsistent)
                    | Ambiguous.Fixed _ -> Ok ()

                let processLayer3D = function
                    | Choice1Of2 (D3 (layerId, layer)) -> Some (Layer3D (layerId, layer))
                    | Choice2Of2 (Sensor.Sensor3D (layerId, sensor)) -> Some (NonHeadLayer3D.Sensor3D (layerId, sensor))
                    | _ -> None

                let processLayer2D = function
                    | Choice1Of2 (D2 (layerId, layer)) -> Some <| Layer2D (layerId, layer)
                    | Choice2Of2 (Sensor.Sensor2D (layerId, sensor)) -> Some <| NonHeadLayer2D.Sensor2D (layerId, sensor)
                    | _ -> None

                let processLayer1D = function
                    | Choice1Of2 (D1 (layerId, layer)) -> Some <| Layer1D (layerId, layer)
                    | Choice2Of2 (Sensor.Sensor1D (layerId, sensor)) -> Some <| NonHeadLayer1D.Sensor1D (layerId, sensor)
                    | _ -> None

                let rec decodeLayer (NotNull "layer schema" schema : Schema.Layer) : Result<Choice<Layer,Sensor>,_> =
                    let createCompatibilityError layer2 compaitibilityError =
                        schema, LayerCompatibilityError {
                            Layer2 = layer2
                            Error = compaitibilityError }

                    let decode processLayer prevLayer =
                        decodeLayer prevLayer
                        |> Result.bind (processLayer >> function
                        | Some layer -> Ok layer
                        | None -> Error (createCompatibilityError prevLayer DimensionMissmatch))

                    let decodeConcatenation processLayer concatenate layers =
                        layers
                        |> Seq.map (
                            Result.mapError (fun error -> schema, LayerError.LayerError [| Inconsistent error|] )
                            >> Result.bind (decode processLayer))
                        |> ResultExtensions.aggregate
                        |> function
                        | Ok layers -> Ok (concatenate layers)
                        | Error errors -> Error (schema, AggregateLayerError errors)

                    tryGetLayerSchema schema <| fun () ->
                        let propsResult =
                            match propsConverter.Decode (schema.Type) with
                            | Some propsDecoder ->
                                propsDecoder.Invoke (schema.Props)
                                |> Result.mapError (PropsConversionError)

                            | None -> Error (InconsistentLayerConversionError.UnknownType (schema.Type))

                        let prevLayerResult =
                            match snd schema.LayerId with
                            | Some prevId -> findLayer prevId |> Result.map Some
                            | None -> Ok None

                        match ResultExtensions.zip prevLayerResult propsResult with
                        | Ok (Some prev, LayerProps.Activator1D activator) ->
                            prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Activation1D (activator, l))))

                            //decodeLayer prev
                            //|> Result.map (function
                            //| Choice1Of2 (D1 (layerId, layer)) -> D1 (fst schema.LayerId, Activation1D (activator, Layer1D (layerId, layer)))
                            //| Choice1Of2 (D2 (layerId, layer)) -> D2 (fst schema.LayerId, Activation2D (activator, Layer2D (layerId, layer)))
                            //| Choice1Of2 (D3 (layerId, layer)) -> D3 (fst schema.LayerId, Activation3D (activator, Layer3D (layerId, layer)))
                            //| Choice2Of2 (Sensor.Sensor1D (layerId, sensor)) -> D1 (fst schema.LayerId, Activation1D (activator, NonHeadLayer1D.Sensor1D (layerId, sensor)))
                            //| Choice2Of2 (Sensor.Sensor2D (layerId, sensor)) -> D2 (fst schema.LayerId, Activation2D (activator, NonHeadLayer2D.Sensor2D (layerId, sensor)))
                            //| Choice2Of2 (Sensor.Sensor3D (layerId, sensor)) -> D3 (fst schema.LayerId, Activation3D (activator, NonHeadLayer3D.Sensor3D (layerId, sensor))))
                            //|> Result.map Choice1Of2

                        | Ok (Some prev, LayerProps.Pooling3D pooling) ->
                            let RefChecked x, RefChecked y, RefChecked z = pooling.Kernel
                            let RefChecked s1, RefChecked s2, RefChecked s3 = pooling.Strides
                            ResultExtensions.aggregate [x; y; z; s1; s2; s3]
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Pooling3D (pooling, l)))))

                        | Ok (Some prev, LayerProps.Convolutional3D conv) ->
                            let (RefChecked f) = conv.Filters
                            let RefChecked x, RefChecked y, RefChecked z = conv.Kernel
                            let RefChecked s1, RefChecked s2, RefChecked s3 = conv.Strides
                            ResultExtensions.aggregate [f; x; y; z; s1; s2; s3]
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Conv3D (conv, l)))))

                        | Ok (Some prev, LayerProps.Flatten3D) ->
                            prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Flatten3D l)))

                        | Ok (Some prev, LayerProps.Pooling2D pooling) ->
                            let RefChecked x, RefChecked y = pooling.Kernel
                            let RefChecked s1, RefChecked s2 = pooling.Strides
                            ResultExtensions.aggregate [x; y; s1; s2]
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Pooling2D (pooling, l)))))

                        | Ok (Some prev, LayerProps.Convolutional2D conv) ->
                            let (RefChecked f) = conv.Filters
                            let RefChecked x, RefChecked y = conv.Kernel
                            let RefChecked s1, RefChecked s2 = conv.Strides
                            ResultExtensions.aggregate [f; x; y; s1; s2]
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Conv2D (conv, l)))))

                        | Ok (Some prev, LayerProps.Flatten2D) ->
                            prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Flatten2D l)))

                        | Ok (Some prev, LayerProps.Dense props) ->
                            ``|RefChecked|`` props.Units
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError [| es |]))
                            |> Result.bind (fun _ -> prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Dense(props, l)))))

                        | Ok (Some prev, LayerProps.Dropout prob) ->
                            prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Dropout(prob, l))))

                        | Ok (_, LayerProps.PrevLayers ids) ->
                            match ids |> Seq.map (findLayer) |> List.ofSeq with
                            | [] | [_] -> Error (schema, LayerError.LayerError [| PrevLayerPropsEmpty |])
                            | layers ->
                                let firstValid : _ option =
                                    layers |> List.tryPick (function
                                        | Ok schema ->
                                            match decodeLayer schema with
                                            | Ok layer -> Some layer
                                            | Error _ -> None
                                        | Error _ -> None)

                                match firstValid with
                                | Some sample ->
                                    match sample with
                                    | Choice1Of2 (D3 _) | Choice2Of2 (Sensor.Sensor3D _) ->
                                        decodeConcatenation processLayer3D (fun layers -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Concatenation3D layers)) layers

                                    | Choice1Of2 (D2 _) | Choice2Of2 (Sensor.Sensor2D _) ->
                                        decodeConcatenation processLayer2D (fun layers -> Choice1Of2 <| D2 (fst schema.LayerId, Layer2D.Concatenation2D layers)) layers

                                    | Choice1Of2 (D1 _) | Choice2Of2 (Sensor.Sensor1D _) ->
                                        decodeConcatenation processLayer1D (fun layers -> Choice1Of2 <| D1 (fst schema.LayerId, Layer1D.Concatenation1D layers)) layers
                                | None ->
                                    // should return aggregate error
                                    decodeConcatenation processLayer3D (fun layers -> Choice1Of2 <| D3 (fst schema.LayerId, Layer3D.Concatenation3D layers)) layers

                        | Ok (_, LayerProps.Sensor3D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor3D (fst schema.LayerId, sensor))
                        | Ok (_, LayerProps.Sensor2D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor2D (fst schema.LayerId, sensor))
                        | Ok (_, LayerProps.Sensor1D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor1D (fst schema.LayerId, sensor))
                        | Ok (None, _) -> Error (schema, LayerError.LayerError [| ExpectedLayerId |])
                        | Error errors -> Error (schema, LayerError.LayerError (Seq.toArray <| Seq.map Inconsistent errors))

                let decodeHead (NotNull "head schema" headSchema : Schema.Head) =
                    let lossResult =
                        lossConverter.DecodeLoss headSchema.Loss
                        |> Result.mapError HeadError.LossError

                    let headActivationResult =
                        propsConverter.ActivatorConverter.DecodeHeadActivation headSchema.Activation
                        |> Result.mapError HeadError.HeadFunctionError

                    let layerResult =
                        match findLayer (headSchema.LayerId) with
                        | Ok (schema) ->
                            decodeLayer schema
                            |> Result.map (fun layer -> schema, layer)
                            |> Result.mapError (fun (layer, error) -> Some layer, error)
                        | Error error ->
                            Error (None, LayerError.LayerError [| Inconsistent error |])

                        |> Result.mapError (HeadError.LayerError)

                    match ResultExtensions.zip layerResult headActivationResult with
                    | Ok ((_, Choice1Of2 (D1 (layerId, layer))), Softmax) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Softmax (headSchema.LossWeight, loss, layerId, layer))
                        |> Result.mapError (Array.singleton)

                    | Ok ((_, Choice1Of2 layer), Activator activator) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Activator (headSchema.LossWeight, loss, layer, activator))
                        |> Result.mapError (Array.singleton)

                    | Ok ((schema, (Choice1Of2 (D2 _ | D3 _) | Choice2Of2 _)), Softmax)
                    | Ok ((schema, Choice2Of2 _), Activator _) ->
                        let layerError = HeadError.LayerError (None, LayerCompatibilityError {
                            Layer2 = schema
                            Error = DimensionMissmatch
                        })

                        match lossResult with
                        | Ok _ -> Error [| layerError |]
                        | Error lossResult -> Error [| lossResult; layerError |]

                    | Error errors ->
                        match lossResult with
                        | Ok _ -> errors
                        | Error lossResult -> lossResult::errors
                        |> List.toArray |> Error

                let layersResult =
                    schema.Layers
                    |> Seq.map (decodeLayer >> Result.mapError (LayerError >> LayerSequenceError))
                    |> ResultExtensions.aggregate

                let headsResult =
                    schema.Heads
                    |> Seq.map (fun head ->
                        decodeHead head
                        |> Result.mapError (fun errors -> HeadError (head, errors)))
                    |> ResultExtensions.aggregate

                let optimizerResult =
                    optimizerConverter.Decode (schema.Optimizer)
                    |> Result.mapError (NetworkConversionError.OptimizerError >> Array.singleton)

                match ResultExtensions.zip3 layersResult headsResult optimizerResult with
                | Ok (_, heads, optimizer) -> Ok { Heads = heads; Optimizer = optimizer; Ambiguities = ambiguities }
                | Error errors -> Error (Seq.concat errors |> Seq.distinct |> Seq.toArray)

            | Error errors -> Error (Array.concat errors |> Array.map LayerSequenceError)

        { new INetworkConverter with
            member _.Encode(NotNull "entity" entity) = encode entity
            member _.Decode(NotNull "network schema" schema) = decode schema }
