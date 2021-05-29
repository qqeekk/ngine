namespace Ngine.Domain.Services.Conversion
open Ngine.Domain.Utils
open Ngine.Domain.Schemas
open System.Collections.Generic
open System

module public NetworkConverters =

    let convert3D = function
        | Layer l -> Choice1Of2 (D3 l)
        | NonHeadLayer.Sensor (id, sensor) -> Choice2Of2 (Sensor.Sensor3D (id, sensor))

    let convert2D = function
        | Layer l -> Choice1Of2 (D2 l)
        | NonHeadLayer.Sensor (id, sensor) -> Choice2Of2 (Sensor.Sensor2D (id, sensor))

    let convert1D = function
        | Layer l -> Choice1Of2 (D1 l)
        | NonHeadLayer.Sensor (id, sensor) -> Choice2Of2 (Sensor.Sensor1D (id, sensor))

    let getLayerId = function
        | Layer (HeadLayer (id, _))
        | NonHeadLayer.Sensor (id, _) -> id

    let getPreviousIds layer =
        let getPreviousIds1D = function
            | Layer1D.Activation1D (_, prev)
            | Layer1D.Dropout (_, prev)
            | Layer1D.Dense (_, prev) -> [getLayerId prev]
            | Layer1D.Flatten2D (prev) -> [getLayerId prev]
            | Layer1D.Flatten3D (prev) -> [getLayerId prev]
            | Layer1D.Concatenation1D (prevs) -> [for p in prevs -> getLayerId p]
            | Layer1D.Empty1D -> []

        let getPreviousIds2D = function
            | Layer2D.Activation2D (_, prev)
            | Layer2D.Pooling2D (_, prev)
            | Layer2D.Conv2D (_, prev) -> [getLayerId prev]
            | Layer2D.Concatenation2D (prevs) -> [for p in prevs -> getLayerId p]
            | Layer2D.Empty2D -> []
        
        let getPreviousIds3D = function
            | Layer3D.Activation3D (_, prev)
            | Layer3D.Pooling3D (_, prev)
            | Layer3D.Conv3D (_, prev) -> [getLayerId prev]
            | Layer3D.Concatenation3D (prevs) -> [for p in prevs -> getLayerId p]
            | Layer3D.Empty3D -> []

        match layer with
        | D1 (HeadLayer (id, prev)) -> id, getPreviousIds1D prev
        | D2 (HeadLayer (id, prev)) -> id, getPreviousIds2D prev
        | D3 (HeadLayer (id, prev)) -> id, getPreviousIds3D prev


    let create (NotNull "props converter" propsConverter : ILayerPropsConverter)
               (NotNull "loss converter" lossConverter : ILossConverter)
               (NotNull "optimizer converter" optimizerConverter : IOptimizerConverter)
               (NotNull "ambiguity converter" ambiguityConverter : IAmbiguityConverter) =

        let encodeLayers (layers: Choice<HeadLayer, Sensor>[]) = 
            let map: Dictionary<Choice<HeadLayer, Sensor>, Schema.Layer option> = Dictionary()
            
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
            
            let rec encode (NotNull "layer" layer: Choice<HeadLayer, Sensor>) : Schema.Layer option =
                let encodeLayer prev props layerId =
                    let prevId = encode prev |> Option.map (fun { LayerId = (prevId, _) } -> prevId)
                    createSchema (layerId, prevId) props
            
                let appendLayerToConcatenation layer =
                    match encode layer with
                    | Some r -> r.LayerId
                    | None -> (0u, 0u), None
            
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
                        | D3 (HeadLayer (layerId, Concatenation3D layers)) ->
                            concatenate (processConcatenatedLayers3D) layerId layers |> Some
            
                        | D3 (HeadLayer (layerId, Layer3D.Conv3D (conv, prev))) ->
                            convert3D prev (LayerProps.Convolutional3D conv) layerId |> Some
            
                        | D3 (HeadLayer (layerId, Layer3D.Pooling3D (poling, prev))) ->
                            convert3D prev (LayerProps.Pooling3D poling) layerId |> Some
            
                        | D3 (HeadLayer (layerId, Layer3D.Activation3D (activator, prev))) ->
                            convert3D prev (LayerProps.Activator3D activator) layerId |> Some
            
                        | D2 (HeadLayer (layerId, Concatenation2D layers)) ->
                            concatenate (processConcatenatedLayers2D) layerId layers |> Some
            
                        | D2 (HeadLayer (layerId, Layer2D.Conv2D (conv, prev))) ->
                            convert2D prev (LayerProps.Convolutional2D conv) layerId |> Some
            
                        | D2 (HeadLayer (layerId, Layer2D.Pooling2D (poling, prev))) ->
                            convert2D prev (LayerProps.Pooling2D poling) layerId |> Some
            
                        | D2 (HeadLayer (layerId, Layer2D.Activation2D (activator, prev))) ->
                            convert2D prev (LayerProps.Activator2D activator) layerId |> Some
            
                        | D1 (HeadLayer (layerId, Layer1D.Flatten3D layer)) ->
                            convert3D layer (LayerProps.Flatten3D) layerId |> Some
            
                        | D1 (HeadLayer (layerId, Layer1D.Flatten2D layer)) ->
                            convert2D layer (LayerProps.Flatten2D) layerId |> Some
            
                        | D1 (HeadLayer (layerId, Layer1D.Dense (dense, layer))) ->
                            convert1D layer (LayerProps.Dense dense) layerId |> Some
            
                        | D1 (HeadLayer (layerId, Layer1D.Activation1D (activator, layer))) ->
                            convert1D layer (LayerProps.Activator1D activator) layerId |> Some
            
                        | D1 (HeadLayer (layerId, Layer1D.Dropout (probability, prev))) ->
                            convert1D prev (LayerProps.Dropout probability) layerId |> Some
            
                        | D1 (HeadLayer (layerId, Concatenation1D layers)) ->
                            concatenate (processConcatenatedLayers1D) layerId layers |> Some
                        
                        | D3 (HeadLayer (_, Empty3D))
                        | D2 (HeadLayer (_, Empty2D))
                        | D1 (HeadLayer (_, Empty1D)) -> None

                    | Choice2Of2 sensor ->
                        match sensor with
                        | Sensor.Sensor3D (layerId, sensor) -> createSchema (layerId, None) (LayerProps.Sensor3D sensor) |> Some
                        | Sensor.Sensor2D (layerId, sensor) -> createSchema (layerId, None) (LayerProps.Sensor2D sensor) |> Some
                        | Sensor.Sensor1D (layerId, sensor) -> createSchema (layerId, None) (LayerProps.Sensor1D sensor) |> Some
            
            do Array.iter (encode >> ignore) layers
            map.Values |> Seq.choose id |> Seq.distinctBy (fun l -> l.LayerId) |> Seq.toArray

        let encodeHeads heads : Schema.Head [] =
            heads
            |> Array.map (function
                | Head.Activator (lossWeight, loss, D1 (HeadLayer (layerId, _)), activator)
                | Head.Activator (lossWeight, loss, D2 (HeadLayer (layerId, _)), activator)
                | Head.Activator (lossWeight, loss, D3 (HeadLayer (layerId, _)), activator) -> {
                    LayerId = layerId
                    Activation = propsConverter.ActivatorConverter.EncodeHeadActivation(HeadFunction.Activator activator)
                    Loss = lossConverter.EncodeLoss loss
                    LossWeight = lossWeight }

                | Head.Softmax (lossWeight, loss, (HeadLayer (layerId, _))) -> {
                    LayerId = layerId
                    Activation = propsConverter.ActivatorConverter.EncodeHeadActivation(HeadFunction.Softmax)
                    Loss = lossConverter.EncodeLoss loss
                    LossWeight = lossWeight
                })


        let encode (entity : Network) : Schema.Network =
            let ambiguities =
                entity.Ambiguities
                |> Seq.map (ambiguityConverter.Encode)
                |> Seq.toArray

            let layers = 
                entity.Heads
                |> Array.map (Choice1Of2 << function
                    | Head.Activator (_, _, layer, _) -> layer
                    | Head.Softmax (_, _, layer) -> D1 layer)
                |> encodeLayers

            let heads : Schema.Head[] = 
                encodeHeads entity.Heads

            { Layers = layers
              Ambiguities = ambiguities
              Heads = heads
              Optimizer = optimizerConverter.Encode(entity.Optimizer) }


        let decodeAmbiguities ambiguities =
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

        let decodeLayers (layers: seq<Schema.Layer>)
                         (ambiguities: seq<Schema.Ambiguity>)
                         : Result<Choice<HeadLayer, Sensor>[] * IDictionary<AmbiguityVariableName, Values<uint32>>, LayerSequenceError<InconsistentLayerConversionError>[]> =
            let ambiguities' = decodeAmbiguities ambiguities

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
                let map: Dictionary<Schema.Layer, Result<Choice<HeadLayer, Sensor>, Schema.Layer * LayerError<InconsistentLayerConversionError>>> = Dictionary()
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
                    | Some (Choice1Of2 (D3 layer)) -> Some (Layer layer)
                    | Some (Choice2Of2 (Sensor.Sensor3D (layerId, sensor))) -> Some <| NonHeadLayer.Sensor (layerId, sensor)
                    | None -> Some <| Layer (HeadLayer ((0u, 0u), Empty3D))
                    | _ -> None

                let processLayer2D = function
                    | Some (Choice1Of2 (D2 layer)) -> Some (Layer layer)
                    | Some (Choice2Of2 (Sensor.Sensor2D (layerId, sensor))) -> Some <| NonHeadLayer.Sensor (layerId, sensor)
                    | None -> Some <| Layer (HeadLayer ((0u, 0u), Empty2D))
                    | _ -> None

                let processLayer1D = function
                    | Some (Choice1Of2 (D1 layer)) -> Some (Layer layer)
                    | Some (Choice2Of2 (Sensor.Sensor1D (layerId, sensor))) -> Some <| NonHeadLayer.Sensor (layerId, sensor)
                    | None -> Some <| Layer (HeadLayer ((0u, 0u), Empty1D))
                    | _ -> None

                let rec decodeLayer (NotNull "layer schema" schema : Schema.Layer) : Result<Choice<HeadLayer,Sensor>,_*_> =
                    let createCompatibilityError layer2 compaitibilityError =
                        schema, LayerCompatibilityError {
                            Layer2 = layer2
                            Error = compaitibilityError }

                    let decode (processLayer: Choice<HeadLayer, Sensor> option -> _) = function
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
                            prev |> decode (processLayer1D >> Option.map (fun l ->
                                Choice1Of2 <| D1 (HeadLayer (fst schema.LayerId, Layer1D.Activation1D (activator, l)))))

                        | Ok (prev, LayerProps.Activator2D activator) ->
                            prev |> decode (processLayer2D >> Option.map (fun l ->
                                Choice1Of2 <| D2 (HeadLayer (fst schema.LayerId, Layer2D.Activation2D (activator, l)))))

                        | Ok (prev, LayerProps.Activator3D activator) ->
                            prev |> decode (processLayer3D >> Option.map (fun l ->
                                Choice1Of2 <| D3 (HeadLayer (fst schema.LayerId, Layer3D.Activation3D (activator, l)))))

                        | Ok (prev, LayerProps.Pooling3D pooling) ->
                            let RefChecked x, RefChecked y, RefChecked z = pooling.Kernel
                            let RefChecked s1, RefChecked s2, RefChecked s3 = pooling.Strides

                            ResultExtensions.aggregate [x; y; z; s1; s2; s3]
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ ->
                                prev |> decode (processLayer3D >> Option.map (fun l ->
                                    Choice1Of2 <| D3 (HeadLayer (fst schema.LayerId, Layer3D.Pooling3D (pooling, l))))))

                        | Ok (prev, LayerProps.Convolutional3D conv) ->
                            let (RefChecked f) = conv.Filters
                            let RefChecked x, RefChecked y, RefChecked z = conv.Kernel
                            let RefChecked s1, RefChecked s2, RefChecked s3 = conv.Strides

                            ResultExtensions.aggregate [f; x; y; z; s1; s2; s3]
                            |> Result.mapError(fun es -> (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer3D >> Option.map (fun l -> 
                                Choice1Of2 <| D3 (HeadLayer (fst schema.LayerId, Layer3D.Conv3D (conv, l))))))

                        | Ok (prev, LayerProps.Flatten3D) ->
                            prev |> decode (processLayer3D >> Option.map (fun l ->
                                Choice1Of2 <| D1 (HeadLayer (fst schema.LayerId, Layer1D.Flatten3D l))))

                        | Ok (prev, LayerProps.Pooling2D pooling) ->
                            let RefChecked x, RefChecked y = pooling.Kernel
                            let RefChecked s1, RefChecked s2 = pooling.Strides

                            ResultExtensions.aggregate [x; y; s1; s2]
                            |> Result.mapError(fun es -> (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer2D >> Option.map (fun l ->
                                Choice1Of2 <| D2 (HeadLayer (fst schema.LayerId, Layer2D.Pooling2D (pooling, l))))))

                        | Ok (prev, LayerProps.Convolutional2D conv) ->
                            let (RefChecked f) = conv.Filters
                            let RefChecked x, RefChecked y = conv.Kernel
                            let RefChecked s1, RefChecked s2 = conv.Strides

                            ResultExtensions.aggregate [f; x; y; s1; s2]
                            |> Result.mapError(fun es -> (schema, LayerError.LayerError es))
                            |> Result.bind (fun _ -> prev |> decode (processLayer2D >> Option.map (fun l ->
                                Choice1Of2 <| D2 (HeadLayer (fst schema.LayerId, Layer2D.Conv2D (conv, l))))))

                        | Ok (prev, LayerProps.Flatten2D) ->
                            prev |> decode (processLayer2D >> Option.map (fun l ->
                                Choice1Of2 <| D1 (HeadLayer (fst schema.LayerId, Layer1D.Flatten2D l))))

                        | Ok (prev, LayerProps.Dense props) ->
                            ``|RefChecked|`` props.Units
                            |> Result.mapError(fun es ->  (schema, LayerError.LayerError [| es |]))
                            |> Result.bind (fun _ -> prev |> decode (processLayer1D >> Option.map (fun l ->
                                Choice1Of2 <| D1 (HeadLayer (fst schema.LayerId, Layer1D.Dense(props, l))))))

                        | Ok (prev, LayerProps.Dropout prob) ->
                            prev |> decode (processLayer1D >> Option.map (fun l ->
                                Choice1Of2 <| D1 (HeadLayer (fst schema.LayerId, Layer1D.Dropout(prob, l)))))

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
                                        decodeConcatenation processLayer3D (fun layers ->
                                            Choice1Of2 <| D3 (HeadLayer (fst schema.LayerId, Layer3D.Concatenation3D layers))) layers

                                    | Choice1Of2 (D2 _) | Choice2Of2 (Sensor.Sensor2D _) ->
                                        decodeConcatenation processLayer2D (fun layers ->
                                            Choice1Of2 <| D2 (HeadLayer (fst schema.LayerId, Layer2D.Concatenation2D layers))) layers

                                    | Choice1Of2 (D1 _) | Choice2Of2 (Sensor.Sensor1D _) ->
                                        decodeConcatenation processLayer1D (fun layers ->
                                            Choice1Of2 <| D1 (HeadLayer (fst schema.LayerId, Layer1D.Concatenation1D layers))) layers
                                | None ->
                                    // should return aggregate error
                                    decodeConcatenation processLayer3D (fun layers ->
                                        Choice1Of2 <| D3 (HeadLayer (fst schema.LayerId, Layer3D.Concatenation3D layers))) layers

                        | Ok (_, LayerProps.Sensor3D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor3D (fst schema.LayerId, sensor))
                        | Ok (_, LayerProps.Sensor2D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor2D (fst schema.LayerId, sensor))
                        | Ok (_, LayerProps.Sensor1D sensor) -> Ok <| Choice2Of2 (Sensor.Sensor1D (fst schema.LayerId, sensor))
                        | Error errors -> Error (schema, LayerError.LayerError (List.toArray errors))

                let layersResult = layers |> Seq.map decodeLayer
                match ResultExtensions.aggregate layersResult with
                | Ok layers -> Ok (layers, upcast ambiguities)
                | Error errors -> Error (errors |> Seq.distinct |> Seq.map LayerSequenceError.LayerError |> Seq.toArray)


        let decodeInconsistent (schema: Schema.Network) =
            let findLayer id =
                schema.Layers |> Seq.find (fun l -> fst l.LayerId = id)

            let layersResult =
                decodeLayers (schema.Layers) (schema.Ambiguities)

            match layersResult with
            | Ok (layers, ambiguities) ->
                let decodeHead (NotNull "head schema" headSchema : Schema.Head) =
                    let lossResult =
                        lossConverter.DecodeLoss headSchema.Loss
                        |> Result.mapError HeadError.LossError

                    let headActivationResult =
                        propsConverter.ActivatorConverter.DecodeHeadActivation headSchema.Activation
                        |> Result.mapError HeadError.HeadFunctionError

                    let layerResult =
                        layers
                        |> Seq.tryPick (function
                            | Choice1Of2 (D1 (HeadLayer (lid, _)) | D2 (HeadLayer (lid, _)) | D3 (HeadLayer (lid, _)))
                            | Choice2Of2 (Sensor.Sensor1D (lid, _) | Sensor.Sensor2D (lid, _) | Sensor.Sensor3D (lid, _))
                                as l when lid = headSchema.LayerId -> Some (Ok l)
                            | _ -> None)
                        |> Option.defaultValue (Error <| HeadError.LayerError
                            (None, LayerError.LayerError [| MissingLayerId headSchema.LayerId |]))

                    match ResultExtensions.zip layerResult headActivationResult with
                    | Ok (Choice1Of2 (D1 layer), HeadFunction.Softmax) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Softmax (headSchema.LossWeight, loss, layer))
                        |> Result.mapError (Array.singleton)

                    | Ok (Choice1Of2 layer, HeadFunction.Activator activator) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Activator (headSchema.LossWeight, loss, layer, activator))
                        |> Result.mapError (Array.singleton)

                    | Ok ((Choice1Of2 (D2 _ | D3 _) | Choice2Of2 _), HeadFunction.Softmax)
                    | Ok ((Choice2Of2 _), HeadFunction.Activator _) ->
                        let layerError = HeadError.LayerError (None, LayerCompatibilityError {
                            Layer2 = findLayer headSchema.LayerId
                            Error = DimensionMissmatch })

                        match lossResult with
                        | Ok _ -> Error [| layerError |]
                        | Error lossResult -> Error [| lossResult; layerError |]

                    | Error errors ->
                        match lossResult with
                        | Ok _ -> errors
                        | Error lossResult -> lossResult::errors
                        |> List.toArray |> Error

                let headsResult =
                    schema.Heads
                    |> Seq.map (fun head ->
                        decodeHead head
                        |> Result.mapError (fun errors -> HeadError (head, errors)))
                    |> ResultExtensions.aggregate

                let optimizerResult =
                    optimizerConverter.Decode (schema.Optimizer)
                    |> Result.mapError (NetworkConversionError.OptimizerError >> Array.singleton)

                match ResultExtensions.zip headsResult optimizerResult with
                | Ok (heads, optimizer) -> Ok {
                    Heads = heads
                    Layers = layers
                    Optimizer = optimizer
                    Ambiguities = ambiguities }
                | Error errors -> Error (Seq.concat errors |> Seq.distinct |> Seq.toArray)

            | Error errors -> Error (Array.map LayerSequenceError errors)


        let decode (schema: Schema.Network) =
            let findLayer id =
                schema.Layers |> Seq.find (fun l -> fst l.LayerId = id)

            let rec isLayerConsistent pid = function
            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Activation1D(_, prev))))
            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Dropout(_, prev))))
            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Dense(_, prev)))) -> isLayerConsistent lid (convert1D prev)
            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Flatten2D prev)))
            | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Conv2D (_, prev))))
            | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Pooling2D (_, prev))))
            | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Activation2D (_, prev)))) -> isLayerConsistent lid (convert2D prev)
            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Flatten3D prev)))
            | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Conv3D (_, prev))))
            | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Pooling3D (_, prev))))
            | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Activation3D (_, prev)))) -> isLayerConsistent lid (convert3D prev)
            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Concatenation1D ([||] | [|_|]))))
            | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Concatenation2D ([||] | [|_|]))))
            | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Concatenation3D ([||] | [|_|])))) ->
                Error [| LayerError (findLayer lid, LayerError.LayerError [| PrevLayerPropsEmpty |]) |]

            | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Concatenation1D prevs))) ->
                prevs
                |> Seq.map (convert1D >> isLayerConsistent lid)
                |> ResultExtensions.aggregate
                |> Result.map (ignore)
                |> Result.mapError (Array.concat)

            | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Concatenation2D prevs))) ->
                prevs
                |> Seq.map (convert2D >> isLayerConsistent lid)
                |> ResultExtensions.aggregate
                |> Result.map (ignore)
                |> Result.mapError (Array.concat)

            | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Concatenation3D prevs))) ->
                prevs
                |> Seq.map (convert3D >> isLayerConsistent lid)
                |> ResultExtensions.aggregate
                |> Result.map (ignore)
                |> Result.mapError (Array.concat)

            | Choice1Of2 (D1 (HeadLayer (_, Layer1D.Empty1D)))
            | Choice1Of2 (D2 (HeadLayer (_, Layer2D.Empty2D)))
            | Choice1Of2 (D3 (HeadLayer (_, Layer3D.Empty3D))) ->
                Error [| LayerError (findLayer pid, LayerError.LayerError [| ExpectedLayerId |]) |]
            | Choice2Of2 _ -> Ok ()

            let rec ilce2lce = function
            | LayerError.LayerError es -> LayerError.LayerError (Array.map Inconsistent es)
            | LayerError.LayerCompatibilityError e -> LayerError.LayerCompatibilityError e
            | LayerError.AggregateLayerError es -> 
                es
                |> Array.map (fun (l, e) -> l, ilce2lce e)
                |> LayerError.AggregateLayerError

            let layersResult =
                decodeLayers (schema.Layers) (schema.Ambiguities)
                |> Result.mapError(Array.map <| function
                    | LayerSequenceError.LayerError (l, e) -> LayerSequenceError.LayerError (l, ilce2lce e)
                    | AmbiguityError (a, es) -> AmbiguityError (a, es))
                |> Result.bind (fun (layers, ambiguites) ->
                    layers
                    |> Seq.map (isLayerConsistent (0u, 0u))
                    |> ResultExtensions.aggregate
                    |> Result.map (fun _ -> layers, ambiguites)
                    |> Result.mapError (Array.concat))

            match layersResult with
            | Ok (layers, ambiguities) ->
                let decodeHead (NotNull "head schema" headSchema : Schema.Head) =
                    let lossResult =
                        lossConverter.DecodeLoss headSchema.Loss
                        |> Result.mapError HeadError.LossError

                    let headActivationResult =
                        propsConverter.ActivatorConverter.DecodeHeadActivation headSchema.Activation
                        |> Result.mapError HeadError.HeadFunctionError

                    let layerResult =
                        layers
                        |> Seq.tryPick (function
                            | Choice1Of2 (D1 (HeadLayer (lid, _)) | D2 (HeadLayer (lid, _)) | D3 (HeadLayer (lid, _)))
                            | Choice2Of2 (Sensor.Sensor1D (lid, _) | Sensor.Sensor2D (lid, _) | Sensor.Sensor3D (lid, _))
                                as l when lid = headSchema.LayerId -> Some (Ok l)
                            | _ -> None)
                        |> Option.defaultValue (Error <| HeadError.LayerError (None, LayerError.LayerError [| Inconsistent (MissingLayerId headSchema.LayerId) |]))

                    match ResultExtensions.zip layerResult headActivationResult with
                    | Ok (Choice1Of2 (D1 layer), HeadFunction.Softmax) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Softmax (headSchema.LossWeight, loss, layer))
                        |> Result.mapError (Array.singleton)

                    | Ok (Choice1Of2 layer, HeadFunction.Activator activator) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Activator (headSchema.LossWeight, loss, layer, activator))
                        |> Result.mapError (Array.singleton)

                    | Ok ((Choice1Of2 (D2 _ | D3 _) | Choice2Of2 _), HeadFunction.Softmax)
                    | Ok ((Choice2Of2 _), HeadFunction.Activator _) ->
                        let layerError = HeadError.LayerError (None, LayerCompatibilityError {
                            Layer2 = findLayer headSchema.LayerId
                            Error = DimensionMissmatch })

                        match lossResult with
                        | Ok _ -> Error [| layerError |]
                        | Error lossResult -> Error [| lossResult; layerError |]

                    | Error errors ->
                        match lossResult with
                        | Ok _ -> errors
                        | Error lossResult -> lossResult::errors
                        |> List.toArray |> Error

                let headsResult =
                    schema.Heads
                    |> Seq.map (fun head ->
                        decodeHead head
                        |> Result.mapError (fun errors -> HeadError (head, errors)))
                    |> ResultExtensions.aggregate

                let optimizerResult =
                    optimizerConverter.Decode (schema.Optimizer)
                    |> Result.mapError (NetworkConversionError.OptimizerError >> Array.singleton)

                match ResultExtensions.zip headsResult optimizerResult with
                | Ok ([||], _) -> Error [| EmptyHeadArrayError |]
                | Ok (heads, optimizer) -> Ok { Heads = heads; Optimizer = optimizer; Ambiguities = ambiguities }
                | Error errors -> Error (Seq.concat errors |> Seq.distinct |> Seq.toArray)

            | Error errors -> Error (Array.map LayerSequenceError errors)

        let encodeInconsistent (schema: InconsistentNetwork): Schema.Network =
            let heads = schema.Heads |> encodeHeads
            let layers = schema.Layers |> encodeLayers
            let ambiguities = [| for a in schema.Ambiguities -> ambiguityConverter.Encode(a) |]
            let optimizer = schema.Optimizer |> optimizerConverter.Encode
            { Heads = heads; Layers = layers; Optimizer = optimizer; Ambiguities = ambiguities }

        let applyAmbiguities (ambiguities: IDictionary<AmbiguityVariableName, uint32>) (network: Schema.Network) =
            let newLayers = [|
                for l in network.Layers ->
                    { l with Props = ambiguityConverter.FindAndReplace(l.Props, fun v -> ambiguities.[v]) }
            |]

            let excludedAmbiguities =
                ambiguities |> Seq.map (fun (KeyValue (Variable name, _)) -> name) |> Set.ofSeq

            { network with 
                Layers = newLayers 
                Ambiguities = Array.where (fun a -> Set.contains (a.Name) excludedAmbiguities |> not) network.Ambiguities }


        { new INetworkConverter with
            member _.AmbiguityConverter: IAmbiguityConverter = ambiguityConverter
            member _.LayerConverter: ILayerPropsConverter = propsConverter
            member _.LossConverter: ILossConverter = lossConverter
            member _.OptimizerConverter: IOptimizerConverter = optimizerConverter

            member _.Encode(NotNull "entity" entity) = encode entity
            member _.EncodeInconsistent(NotNull "network schema" schema) = encodeInconsistent schema
            member _.Decode(NotNull "network schema" schema) = decode schema
            member _.DecodeInconsistent(NotNull "network schema" schema) = decodeInconsistent schema
            member _.ApplyAmbiguities(ambiguties, network) = applyAmbiguities ambiguties network
        }
