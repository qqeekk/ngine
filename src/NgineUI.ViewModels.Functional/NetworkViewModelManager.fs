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
open System.Collections.Generic
open NgineUI.ViewModels
open NgineUI.ViewModels.Network.Ambiguities
open System.Windows

module NetworkViewModelManager =
    let encode (networkVM: NetworkViewModel) (ambiguities: IEnumerable<Ambiguity>) (optimizer: Optimizer) : InconsistentNetwork =
        let nodesSeparated =
            networkVM.Nodes.Items
            |> Seq.cast<NgineNodeViewModel>
            |> Seq.map (fun n -> n.GetValue())
            |> Seq.toArray

        let layers =
            nodesSeparated
            |> Array.choose (function
                | Choice2Of3 headLayer -> Some (Choice1Of2 headLayer)
                | Choice3Of3 sensor -> Some (Choice2Of2 sensor)
                | _ -> None)

        let heads =
            nodesSeparated
            |> Array.choose (function
                | Choice1Of3 head -> Some head
                | _ -> None)

        let ambiguities = Dictionary ambiguities

        { Layers = layers
          Ambiguities = ambiguities
          Heads = heads
          Optimizer = optimizer }

    let decode (converter: INetworkConverter) (network: InconsistentNetwork) =
        let connections =
            network.Layers
            |> Seq.collect(function
                | Choice1Of2 l ->
                    let (id, prevs) = NetworkConverters.getPreviousIds l
                    [for p in prevs -> id, p]
                | Choice2Of2 _ -> [])

        let layerIdTracker =
            network.Layers
            |> Seq.map (function
                | Choice1Of2 (D1 (HeadLayer (id, _)))
                | Choice1Of2 (D2 (HeadLayer (id, _)))
                | Choice1Of2 (D3 (HeadLayer (id, _)))
                | Choice2Of2 (Sensor1D (id, _))
                | Choice2Of2 (Sensor2D (id, _))
                | Choice2Of2 (Sensor3D (id, _)) -> id.ToValueTuple())
            |> LayerIdTracker

        let ambiguitiesVM = AmbiguityListViewModel(converter.AmbiguityConverter)
        do ambiguitiesVM.Fill (network.Ambiguities)

        let nodes =
            network.Layers
            |> Array.map<_, NgineNodeViewModel> (function
                | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Activation1D(a, _)))) ->
                    let vm = Activation1DViewModel(converter.LayerConverter.ActivatorConverter, layerIdTracker, false)
                    vm.Id <- lid; vm.Setup(a); upcast vm
                | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Activation2D(a, _)))) ->
                    let vm = Activation2DViewModel(converter.LayerConverter.ActivatorConverter, layerIdTracker, false)
                    vm.Id <- lid; vm.Setup(a); upcast vm
                | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Activation3D(a, _)))) ->
                    let vm = Activation3DViewModel(converter.LayerConverter.ActivatorConverter, layerIdTracker, false)
                    vm.Id <- lid; vm.Setup(a); upcast vm
                | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Concatenation1D _))) ->
                    let vm = Concatenation1DViewModel(layerIdTracker, false)
                    vm.Id <- lid; upcast vm
                | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Concatenation2D _))) ->
                    let vm = Concatenation2DViewModel(layerIdTracker, false)
                    vm.Id <- lid; upcast vm
                | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Concatenation3D _))) ->
                    let vm = Concatenation3DViewModel(layerIdTracker, false)
                    vm.Id <- lid; upcast vm
                | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Conv2D(conv, _)))) ->
                    let vm = Conv2DViewModel(layerIdTracker, ambiguitiesVM, false)
                    vm.Id <- lid; vm.Setup(conv); upcast vm
                | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Conv3D(conv, _)))) ->
                    let vm = Conv3DViewModel(layerIdTracker, ambiguitiesVM, false)
                    vm.Id <- lid; vm.Setup(conv); upcast vm
                | Choice1Of2 (D2 (HeadLayer (lid, Layer2D.Pooling2D(p, _)))) ->
                    let vm = Pooling2DViewModel(layerIdTracker, ambiguitiesVM, false)
                    vm.Id <- lid; vm.Setup(p); upcast vm
                | Choice1Of2 (D3 (HeadLayer (lid, Layer3D.Pooling3D(p, _)))) ->
                    let vm = Pooling3DViewModel(layerIdTracker, ambiguitiesVM, false)
                    vm.Id <- lid; vm.Setup(p); upcast vm
                | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Dense(units, _)))) ->
                    let vm = DenseViewModel(layerIdTracker, ambiguitiesVM, false)
                    vm.Id <- lid; vm.Setup(units); upcast vm
                | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Flatten2D(_)))) ->
                    let vm = Flatten2DViewModel(layerIdTracker, false)
                    vm.Id <- lid; upcast vm
                | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Flatten3D(_)))) ->
                    let vm = Flatten3DViewModel(layerIdTracker, false)
                    vm.Id <- lid; upcast vm

                | Choice2Of2 (Sensor1D (lid, input)) ->
                    let vm = Input1DViewModel(layerIdTracker, false)
                    vm.Id <- lid; vm.Setup(input); upcast vm
                | Choice2Of2 (Sensor2D (lid, input)) ->
                    let vm = Input2DViewModel(layerIdTracker, false)
                    vm.Id <- lid; vm.Setup(input); upcast vm
                | Choice2Of2 (Sensor3D (lid, input)) ->
                    let vm = Input3DViewModel(layerIdTracker, false)
                    vm.Id <- lid; vm.Setup(input); upcast vm

                | Choice1Of2 (D1 (HeadLayer (_, Layer1D.Empty1D)))
                | Choice1Of2 (D2 (HeadLayer (_, Layer2D.Empty2D)))
                | Choice1Of2 (D3 (HeadLayer (_, Layer3D.Empty3D))) ->
                    raise <| new InvalidOperationException()

                // TODO: Implement
                | Choice1Of2 (D1 (HeadLayer (lid, Layer1D.Dropout(d, _)))) ->
                    raise <| new NotImplementedException())

        let getPosition ((depth, order): LayerId) =
            Point((float depth - 1.) * 300., (float order - 1.) * 500.)

        for n in nodes do
            n.Position <- getPosition n.Id

        let headNodes =
            network.Heads
            |> Array.map<_, _ * NgineNodeViewModel> (fun h ->
                match h with
                | Head.Softmax (_, _, HeadLayer ((depth, order), _))
                | Head.Activator(_, _, (D1 (HeadLayer ((depth, order), _))), _) ->
                    let vm = new Head1DViewModel(converter.LayerConverter.ActivatorConverter, converter.LossConverter)
                    vm.Position <- getPosition (depth + 1u, order)
                    vm.Setup(h); (depth, order), upcast vm

                | Head.Activator(_, _, (D2 (HeadLayer ((depth, order), _))), _) ->
                    let vm = new Head2DViewModel(converter.LayerConverter.ActivatorConverter, converter.LossConverter)
                    vm.Position <- getPosition (depth + 1u, order)
                    vm.Setup(h); (depth, order), upcast vm

                | Head.Activator(_, _, (D3 (HeadLayer ((depth, order), _))), _) as h ->
                    let vm = new Head3DViewModel(converter.LayerConverter.ActivatorConverter, converter.LossConverter)
                    vm.Position <- getPosition (depth + 1u, order)
                    vm.Setup(h); (depth, order), upcast vm)

        let nodesById =
            seq { for n in nodes -> n.Id, n :> NodeViewModel }
            |> dict

        let tryGetNode id =
            match nodesById.TryGetValue(id) with
            | true, x -> Some x
            | false, _ -> None

        let networkVM = new NetworkViewModel()
        let tryConnect lin =
            Option.map (fun lout -> networkVM.ConnectionFactory.Invoke(lin, lout))

        let getNthInput n (node: NodeViewModel) =
            node.Inputs.Items |> Seq.filter (fun i -> i.Port.IsVisible) |> Seq.item n
        
        let getNthOutput n (node: NodeViewModel) =
            node.Outputs.Items |> Seq.filter (fun i -> i.Port.IsVisible) |> Seq.item n

        let connections =
            [for (pid, h) in headNodes ->
                let hin = getNthInput 0 h
                let lout = tryGetNode pid |> Option.map (getNthOutput 1) // Head output
                tryConnect hin lout]
            @
            [for (inId, outId) in connections ->
                let lin = nodesById.[inId] |> getNthInput 0
                let lout = tryGetNode outId |> Option.map (getNthOutput 0)
                tryConnect lin lout]
            |> List.choose id

        networkVM.Nodes.AddRange(Seq.concat [nodes; Array.map snd headNodes] |> Seq.cast<NodeViewModel>)
        for con in connections do
            networkVM.Connections.Edit(fun x -> x.Add(con))

        // Activate Id generator
        for n in nodes do n.EnableIdGenerator()

        struct (networkVM, ambiguitiesVM, layerIdTracker)


    let instance converter = {
        new INetworkPartsConverter with
            member __.Encode(network, ambiguities, optimizer) = encode network ambiguities optimizer
            member __.Decode(schema) = decode converter schema
        }
