namespace Ngine.Domain.Services.Conversion
open Ngine.Domain.Utils
open Ngine.Domain.Schemas
open System.Collections.Generic

module public NetworkConverters =

    let create (NotNull "kernel converter" kernelConverter : ILayerPropsConverter) =

        let encode (entity : Network) : Schema.Network =
            let map: Dictionary<Choice<Layer, Sensor>, Schema.Layer> = Dictionary()
            let maxBranchTracker: Dictionary<uint32, uint32> = Dictionary([KeyValuePair (1u, 0u)])

            let tryGetLayerSchema layer (create: unit -> _) : _ =
                match map.TryGetValue layer with
                | true, res -> res
                | false, _ ->
                    let result = create()
                    do map.[layer] <- result
                    do maxBranchTracker.[fst result.LayerId] <- snd result.LayerId
                    result

            let createSchema (prevLayerId: Schema.LayerId option) props : Schema.Layer =
                let row =
                    match prevLayerId with
                    | Some (row, _) -> row + 1u
                    | None -> 1u

                { LayerId = (row, maxBranchTracker.[row] + 1u)
                  PreviousLayerId = prevLayerId
                  Type = kernelConverter.EncodeLayerType props
                  Props = kernelConverter.Encode props }

            let concatenate processConcatenatedLayers layers =
                let prevLayers : Schema.LayerId [] =
                    layers |> Array.map (processConcatenatedLayers)

                { createSchema (Seq.maxBy fst prevLayers |> Some) (PrevLayers prevLayers)
                    with PreviousLayerId = None }

            let rec encode (NotNull "layer" layer: Choice<Layer, Sensor>) =
                let encodeLayer layer props =
                    let r : Schema.Layer = encode layer
                    createSchema (Some r.LayerId) props

                let appendLayerToConcatenation layer =
                    let r = encode layer
                    r.LayerId

                let convert3D = encodeLayer << function
                    | Layer3D layer -> Choice1Of2 (D3 layer)
                    | NonHeadLayer3D.Sensor3D sensor -> Choice2Of2 (Sensor3D sensor)

                let convert2D = encodeLayer << function
                    | Layer2D layer -> Choice1Of2 (D2 layer)
                    | NonHeadLayer2D.Sensor2D sensor -> Choice2Of2 (Sensor2D sensor)

                let convert1D = encodeLayer << function
                    | Layer1D layer -> Choice1Of2 (D1 layer)
                    | NonHeadLayer1D.Sensor1D sensor -> Choice2Of2 (Sensor1D sensor)

                let processConcatenatedLayers1D = appendLayerToConcatenation << function
                    | Layer1D layer -> Choice1Of2 (D1 layer)
                    | NonHeadLayer1D.Sensor1D sensor -> Choice2Of2 (Sensor1D sensor)
                    
                let processConcatenatedLayers2D = appendLayerToConcatenation << function
                    | Layer2D layer -> Choice1Of2 (D2 layer)
                    | NonHeadLayer2D.Sensor2D sensor -> Choice2Of2 (Sensor2D sensor)

                let processConcatenatedLayers3D = appendLayerToConcatenation << function
                    | Layer3D layer -> Choice1Of2 (D3 layer)
                    | NonHeadLayer3D.Sensor3D sensor -> Choice2Of2 (Sensor3D sensor)

                tryGetLayerSchema layer <| fun () ->
                    match layer with
                    | Choice1Of2 layer ->
                        match layer with
                        | D3 (Concatenation3D layers) ->
                            concatenate (processConcatenatedLayers3D) layers

                        | D3 (Layer3D.Conv3D (conv, prev)) ->
                            convert3D prev (LayerProps.Convolutional3D conv)

                        | D3 (Layer3D.Pooling3D (poling, prev)) ->
                            convert3D prev (LayerProps.Pooling3D poling)

                        | D3 (Layer3D.Activation3D (activator, prev)) ->
                            convert3D prev (LayerProps.Activator activator)

                        | D2 (Concatenation2D layers) ->
                            concatenate (processConcatenatedLayers2D) layers

                        | D2 (Layer2D.Conv2D (conv, prev)) ->
                            convert2D prev (LayerProps.Convolutional2D conv)

                        | D2 (Layer2D.Pooling2D (poling, prev)) ->
                            convert2D prev (LayerProps.Pooling2D poling)

                        | D2 (Layer2D.Activation2D (activator, prev)) ->
                            convert2D prev (LayerProps.Activator activator)

                        | D1 (Layer1D.Flatten3D layer) ->
                            convert3D layer (LayerProps.Flatten3D)

                        | D1 (Layer1D.Flatten2D layer) ->
                            convert2D layer (LayerProps.Flatten2D)

                        | D1 (Layer1D.Dense (dense, layer)) ->
                            convert1D layer (LayerProps.Dense dense)

                        | D1 (Layer1D.Activation1D (activator, layer)) ->
                            convert1D layer (LayerProps.Activator activator)

                        | D1 (Layer1D.Dropout (probability, prev)) ->
                            convert1D prev (LayerProps.Dropout probability)
                            
                        | D1 (Concatenation1D layers) ->
                            concatenate (processConcatenatedLayers1D) layers

                    | Choice2Of2 sensor ->
                        createSchema None (LayerProps.Sensor sensor)

                        
            let heads : Schema.Head[]=
                entity.Heads
                |> Array.map (function
                    | Head.Activator (loss, layer, activator) -> {
                        LayerId = (encode <| Choice1Of2 layer).LayerId
                        Activation = kernelConverter.EncodeHeadActivation(HeadFunction.Activator activator)
                        Loss = kernelConverter.EncodeLoss loss }
                    | Head.Softmax (loss, layer) -> {
                        LayerId = (encode <| Choice1Of2 (D1 layer)).LayerId
                        Activation = kernelConverter.EncodeHeadActivation(HeadFunction.Softmax)
                        Loss = kernelConverter.EncodeLoss loss
                    })

            { Layers = map.Values |> Seq.toArray
              Heads = heads
              Optimizer = kernelConverter.EncodeOptimizer entity.Optimizer }

        let decode (schema: Schema.Network) =
            let idmap: Dictionary<Schema.LayerId, Schema.Layer> = Dictionary()
            let idmapPopulatingResult =
                (Ok (), schema.Layers)
                ||> Seq.fold (fun prevResult layer ->
                    match idmap.TryGetValue (layer.LayerId) with
                    | true, existing ->
                        let error = layer, LayerCompatibilityError {
                            Layer2 = existing
                            Error = DuplicateLayerId
                        }
                        match prevResult with
                        | Ok _ -> Error [error]
                        | Error prev -> Error (error::prev)

                    | false, _ ->
                        Ok (idmap.[layer.LayerId] <- layer))

            match idmapPopulatingResult with
            | Ok _ ->
                let map: Dictionary<Schema.Layer, Result<Choice<Layer, Sensor>, Schema.Layer * LayerError>> = Dictionary()
                let tryGetLayerSchema layer (create: unit -> Result<_, _>) =
                    match map.TryGetValue layer with
                    | true, res -> res
                    | false, _ ->
                        let result = create()
                        do map.[layer] <-result
                        result

                let findLayer (id : Schema.LayerId) =
                    match idmap.TryGetValue id with
                    | true, layer -> Ok (layer)
                    | false, _ -> Error (MissingLayerId id)

                let processLayer3D = function
                    | Choice1Of2 (D3 layer) -> Some (Layer3D layer)
                    | Choice2Of2 (Sensor3D sensor) -> Some (NonHeadLayer3D.Sensor3D sensor)
                    | _ -> None

                let processLayer2D = function
                    | Choice1Of2 (D2 layer) -> Some <| Layer2D layer
                    | Choice2Of2 (Sensor2D sensor) -> Some <| NonHeadLayer2D.Sensor2D sensor
                    | _ -> None

                let processLayer1D = function
                    | Choice1Of2 (D1 layer) -> Some <| Layer1D layer
                    | Choice2Of2 (Sensor1D sensor) -> Some <| NonHeadLayer1D.Sensor1D sensor
                    | _ -> None

                let rec decodeLayer (NotNull "layer schema" schema : Schema.Layer) : Result<Choice<Layer,Sensor>,_> =
                    let createCompatibilityError layer2 compaitibilityError =
                        schema, LayerCompatibilityError {
                            Layer2 = layer2
                            Error = compaitibilityError
                        }

                    let decode processLayer prevLayer =
                        decodeLayer prevLayer
                        |> Result.bind (processLayer >> function
                        | Some layer -> Ok layer
                        | None -> Error (createCompatibilityError prevLayer DimensionMissmatch))

                    let decodeConcatenation processLayer concatenate layers =
                        layers
                        |> Seq.map (
                            Result.mapError (fun error -> schema, LayerError.LayerError [|error|] )
                            >> Result.bind (decode processLayer))
                        |> ResultExtensions.aggregate
                        |> function
                        | Ok layers -> Ok (concatenate layers)
                        | Error errors -> Error (schema, AggregateLayerError errors)

                    tryGetLayerSchema schema <| fun () ->
                        let propsResult =
                            match kernelConverter.Decode (schema.Type) with
                            | Some propsDecoder ->
                                propsDecoder.Invoke (schema.Props)
                                |> Result.mapError (PropsConversionError)

                            | None -> Error (LayerConversionError.UnknownType (schema.Type))

                        let prevLayerResult =
                            match schema.PreviousLayerId with
                            | Some prevId -> findLayer prevId |> Result.map Some
                            | None -> Ok None

                        match ResultExtensions.zip prevLayerResult propsResult with
                        | Ok (Some layer, LayerProps.Activator activator) ->
                            decodeLayer layer
                            |> Result.map (function
                            | Choice1Of2 (D1 layer) -> D1 <| Activation1D (activator, Layer1D layer)
                            | Choice1Of2 (D2 layer) -> D2 <| Activation2D (activator, Layer2D layer)
                            | Choice1Of2 (D3 layer) -> D3 <| Activation3D (activator, Layer3D layer)
                            | Choice2Of2 (Sensor1D sensor) -> D1 <| Activation1D (activator, NonHeadLayer1D.Sensor1D sensor)
                            | Choice2Of2 (Sensor2D sensor) -> D2 <| Activation2D (activator, NonHeadLayer2D.Sensor2D sensor)
                            | Choice2Of2 (Sensor3D sensor) -> D3 <| Activation3D (activator, NonHeadLayer3D.Sensor3D sensor))
                            |> Result.map Choice1Of2

                        | Ok (Some prev, LayerProps.Pooling3D pooling) ->
                            prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (Layer3D.Pooling3D (pooling, l))))

                        | Ok (Some prev, LayerProps.Convolutional3D conv) ->
                            prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D3 (Layer3D.Conv3D (conv, l))))

                        | Ok (Some prev, LayerProps.Flatten3D) ->
                            prev |> decode (processLayer3D >> Option.map (fun l -> Choice1Of2 <| D1 (Layer1D.Flatten3D l)))

                        | Ok (Some prev, LayerProps.Pooling2D pooling) ->
                            prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (Layer2D.Pooling2D (pooling, l))))

                        | Ok (Some prev, LayerProps.Convolutional2D conv) ->
                            prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D2 (Layer2D.Conv2D (conv, l))))

                        | Ok (Some prev, LayerProps.Flatten2D) ->
                            prev |> decode (processLayer2D >> Option.map (fun l -> Choice1Of2 <| D1 (Layer1D.Flatten2D l)))

                        | Ok (Some prev, LayerProps.Dense props) ->
                            prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (Layer1D.Dense(props, l))))
                            
                        | Ok (Some prev, LayerProps.Dropout prob) ->
                            prev |> decode (processLayer1D >> Option.map (fun l -> Choice1Of2 <| D1 (Layer1D.Dropout(prob, l))))

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
                                    | Choice1Of2 (D3 _) | Choice2Of2 (Sensor3D _) ->
                                        decodeConcatenation processLayer3D (Choice1Of2 << D3 << Layer3D.Concatenation3D) layers

                                    | Choice1Of2 (D2 _) | Choice2Of2 (Sensor2D _) ->
                                        decodeConcatenation processLayer2D (Choice1Of2 << D2 << Layer2D.Concatenation2D) layers

                                    | Choice1Of2 (D1 _) | Choice2Of2 (Sensor1D _) ->
                                        decodeConcatenation processLayer2D (Choice1Of2 << D2 << Layer2D.Concatenation2D) layers
                                | None ->
                                    // should return aggregate error
                                    decodeConcatenation processLayer3D (Choice1Of2 << D3 << Layer3D.Concatenation3D) layers

                        | Ok (_, LayerProps.Sensor sensor) -> Ok <| Choice2Of2 sensor
                        | Ok (None, _) -> Error (schema, LayerError.LayerError [| ExpectedLayerId |])
                        | Error errors -> Error (schema, LayerError.LayerError (List.toArray errors))

                let decodeHead (NotNull "head schema" headSchema : Schema.Head) =
                    let lossResult =
                        kernelConverter.DecodeLoss headSchema.Loss
                        |> Result.mapError HeadError.LossError

                    let headActivationResult =
                        kernelConverter.DecodeHeadActivation headSchema.Activation
                        |> Result.mapError HeadError.HeadFunctionError

                    let layerResult =
                        match findLayer (headSchema.LayerId) with
                        | Ok (schema) ->
                            decodeLayer schema
                            |> Result.map (fun layer -> schema, layer)
                            |> Result.mapError (fun (layer, error) -> Some layer, error)
                        | Error error ->
                            Error (None, LayerError.LayerError [| error |])

                        |> Result.mapError (HeadError.LayerError)

                    match ResultExtensions.zip layerResult headActivationResult with
                    | Ok ((_, Choice1Of2 (D1 layer)), Softmax) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Softmax (loss, layer))
                        |> Result.mapError (Array.singleton)

                    | Ok ((_, Choice1Of2 layer), Activator activator) ->
                        lossResult
                        |> Result.map (fun loss -> Head.Activator (loss, layer, activator))
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
                    |> Seq.map (decodeLayer >> Result.mapError LayerError)
                    |> ResultExtensions.aggregate

                let headsResult =
                    schema.Heads
                    |> Seq.map (fun head ->
                        decodeHead head
                        |> Result.mapError (fun errors -> HeadError (head, errors)))
                    |> ResultExtensions.aggregate

                let optimizerResult =
                    kernelConverter.DecodeOptimizer (schema.Optimizer)
                    |> Result.mapError (NetworkConversionError.OptimizerError >> Array.singleton)

                match ResultExtensions.zip3 layersResult headsResult optimizerResult with
                | Ok (_, heads, optimizer) -> Ok { Heads = heads; Optimizer = optimizer }
                | Error errors -> Error (Seq.concat errors |> Seq.distinct |> Seq.toArray)

            | Error errors -> Error (errors |> Seq.map (NetworkConversionError.LayerError) |> Seq.toArray)

        { new INetworkConverter with
            member _.Encode(NotNull "entity" entity) = encode entity
            member _.Decode(NotNull "network schema" schema) = decode schema }
