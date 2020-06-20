namespace NgineUI.ViewModels.Functional

open NgineUI.ViewModels.AppServices.Abstract
open NgineUI.ViewModels.Network.Nodes
open Ngine.Infrastructure.AppServices
open NgineUI.ViewModels.Network
open Ngine.Domain.Services.Conversion
open System
open DynamicData
open NodeNetwork.ViewModels
open Ngine.Domain.Schemas

module NetworkManager =
    let encode (converter: INetworkConverter) (parts: NetworkPartsDto) : Schema.Network =
        let nodesSeparated =
            parts.Nodes
            |> Array.map (fun n -> n.GetValue())

        let layers =
            nodesSeparated
            |> Array.choose (function
                | Choice2Of3 headLayer -> Some (Choice1Of2 headLayer)
                | Choice3Of3 sensor -> Some (Choice2Of2 sensor)
                | _ -> None)
            |> converter.EncodeLayers

        let heads =
            nodesSeparated
            |> Array.choose (function
                | Choice1Of3 head -> Some head
                | _ -> None)
            |> converter.EncodeHeads

        let ambiguities =
            parts.Ambiguities.Items
            |> Seq.toArray

        {
            Layers = layers
            Heads = heads
            Ambiguities = ambiguities
            Optimizer = parts.Optimizer
        }

    let decode (converter: INetworkConverter) (schema: Schema.Network) =
        let networkResult = converter.DecodeInconsistent (schema)
        let connectionsMutable =
            schema.Layers
            |> Seq.choose(fun { LayerId = (lid, prevId) } -> 
                match prevId with
                | Some pid -> Some (lid, pid)
                | _ -> None)
            |> ResizeArray

        let layerIdTracker = LayerIdTracker(seq {
            for l in schema.Layers -> (fst l.LayerId).ToValueTuple()})

        match networkResult with
        | Ok network ->
            let ambiguities =
                let vm = AmbiguitiesViewModel()
                for item in network.Ambiguities do
                    vm.Add(converter.AmbiguityConverter.Encode item)
                vm

            let nodes: seq<NgineNodeViewModel> =
                network.Layers
                |> Seq.map (function
                    | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Activation1D(a, _)))) ->
                        let vm = Activation1DViewModel(converter.LayerConverter.ActivatorConverter, layerIdTracker, false)
                        vm.Id <- lid; vm.Setup(a); upcast vm
                    | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Activation2D(a, _)))) ->
                        let vm = Activation2DViewModel(converter.LayerConverter.ActivatorConverter, layerIdTracker, false)
                        vm.Id <- lid; vm.Setup(a); upcast vm
                    | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Activation3D(a, _)))) ->
                        let vm = Activation3DViewModel(converter.LayerConverter.ActivatorConverter, layerIdTracker, false)
                        vm.Id <- lid; vm.Setup(a); upcast vm
                    | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Concatenation1D(prev)))) ->
                        let vm = Concatenation1DViewModel(layerIdTracker, false)
                        connectionsMutable.AddRange([for p in prev -> lid, NetworkConverters.getLayerId p])
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Concatenation2D(prev)))) ->
                        let vm = Concatenation2DViewModel(layerIdTracker, false)
                        connectionsMutable.AddRange([for p in prev -> lid, NetworkConverters.getLayerId p])
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Concatenation3D(prev)))) ->
                        let vm = Concatenation3DViewModel(layerIdTracker, false)
                        connectionsMutable.AddRange([for p in prev -> lid, NetworkConverters.getLayerId p])
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Conv2D(conv, _)))) ->
                        let vm = Conv2DViewModel(layerIdTracker, ambiguities.Names, false)
                        vm.Setup(conv)
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Conv3D(conv, _)))) ->
                        let vm = Conv3DViewModel(layerIdTracker, ambiguities.Names, false)
                        vm.Setup(conv)
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Pooling2D(p, _)))) ->
                        let vm = Pooling2DViewModel(layerIdTracker, ambiguities.Names, false)
                        vm.Setup(p)
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Pooling3D(p, _)))) ->
                        let vm = Pooling3DViewModel(layerIdTracker, ambiguities.Names, false)
                        vm.Setup(p)
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Dense(units, _)))) ->
                        let vm = DenseViewModel(layerIdTracker, ambiguities.Names, false)
                        vm.Setup(units)
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Flatten2D(_)))) ->
                        let vm = Flatten2DViewModel(layerIdTracker, false)
                        vm.Id <- lid; upcast vm
                    | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Flatten3D(_)))) ->
                        let vm = Flatten3DViewModel(layerIdTracker, false)
                        vm.Id <- lid; upcast vm

                    | Choice2Of2 (Sensor1D (lid, input)) ->
                        let vm = Input1DViewModel(layerIdTracker, false)
                        vm.Setup(input)
                        vm.Id <- lid; upcast vm
                    | Choice2Of2 (Sensor2D (lid, input)) ->
                        let vm = Input2DViewModel(layerIdTracker, false)
                        vm.Setup(input)
                        vm.Id <- lid; upcast vm
                    | Choice2Of2 (Sensor3D (lid, input)) ->
                        let vm = Input3DViewModel(layerIdTracker, false)
                        vm.Setup(input)
                        vm.Id <- lid; upcast vm

                    | Choice1Of2 (D1 (HeadLayer (_, Layer1D.Empty1D)))
                    | Choice1Of2 (D2 (HeadLayer (_, Layer2D.Empty2D)))
                    | Choice1Of2 (D3 (HeadLayer (_, Layer3D.Empty3D))) ->
                        raise <| new InvalidOperationException()

                    // TODO: Implement
                    | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Dropout(d, _)))) ->
                        raise <| new NotImplementedException())
            
            let headNodes: seq<LayerId * NgineNodeViewModel> =
                network.Heads
                |> Seq.map (fun h ->
                    match h with
                    | Head.Softmax (_, _, HeadLayer (lid, _))
                    | Head.Activator(_, _, (D1 (HeadLayer (lid, _))), _) ->
                        let vm = new Head1DViewModel(converter.LayerConverter.ActivatorConverter, converter.LossConverter)
                        vm.Setup(h); lid, upcast vm

                    | Head.Activator(_, _, (D2 (HeadLayer (lid, _))), _) ->
                        let vm = new Head2DViewModel(converter.LayerConverter.ActivatorConverter, converter.LossConverter)
                        vm.Setup(h); lid, upcast vm

                    | Head.Activator(_, _, (D3 (HeadLayer (lid, _))), _) as h ->
                        let vm = new Head3DViewModel(converter.LayerConverter.ActivatorConverter, converter.LossConverter)
                        vm.Setup(h); lid, upcast vm
                )

            let nodesById = 
                seq { for n in nodes -> n.Id, n :> NodeViewModel }
                |> dict

            let nodeNetwork = new NetworkViewModel()
            let connections =
                [ for (pid, h) in headNodes -> 
                    let hin = h.VisibleInputs.Items |> Seq.item 0
                    let lout = nodesById.[pid].VisibleOutputs.Items |> Seq.item 1 // Head output
                    nodeNetwork.ConnectionFactory.Invoke(hin, lout) ]
                @
                [ for (inId, outId) in connectionsMutable ->
                    let lin = nodesById.[inId].VisibleInputs.Items |> Seq.item 0
                    let lout = nodesById.[outId].VisibleOutputs.Items |> Seq.item 0
                    nodeNetwork.ConnectionFactory.Invoke(lin, lout) ]

            nodeNetwork.Nodes.AddRange(Seq.concat [nodes; Seq.map snd headNodes] |> Seq.cast<NodeViewModel>)
            nodeNetwork.Connections.AddRange(connections)

            // Activate Id generator
            for n in nodes do n.EnableIdGenerator()
            Ok struct (nodeNetwork, ambiguities)
        | Error error -> Error error
                

    let instance converter = {
        new INetworkPartsConverter with
            member __.Encode(parts) = encode converter parts
            member __.Decode(schema) = decode converter schema
        }
        
