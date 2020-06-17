namespace NgineUI.ViewModels.Functional
open NgineUI.ViewModels.AppServices.Abstract
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

    let instance converter = {
        new INetworkPartsConverter with
            member __.Encode(parts) = encode converter parts
        }
        
