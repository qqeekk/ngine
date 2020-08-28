namespace Ngine.Backend.Converters

open System.Collections.Generic
open Keras
open Keras.Layers

module internal NetworkConverter =
    open Keras.Models
    open Ngine.Backend.Converters
    open Ngine.Domain.Schemas

    let internal keras network =
        let map: Dictionary<Choice<HeadLayer, Sensor>, Keras.Layers.BaseLayer> = Dictionary()

        let tryGetLayerSchema layer inputs (create: unit -> BaseLayer * BaseLayer list) : (BaseLayer * BaseLayer list) =
            match map.TryGetValue layer with
            | true, res -> res, inputs
            | false, _ ->
                let result, inputs = create()
                do map.[layer] <- result
                result, inputs

        let layers, inputs = ([], network.Heads) ||> Array.mapFold (fun inputs -> function
            | Head.Activator(p, loss, layer, activation) ->
                let layer, inputs = KernelConverter.keras (Choice1Of2 layer) tryGetLayerSchema inputs (network.Ambiguities)
                let activation = ActivatorConverter.kerasHeadActivation (HeadFunction.Activator activation)
                let loss = LossConverter.keras loss
                (p, loss, activation.Set [| layer |]), inputs

            | Head.Softmax (p, loss, layer) ->
                let layer, inputs = KernelConverter.keras (Choice1Of2 <| D1 layer) tryGetLayerSchema inputs (network.Ambiguities)
                let activation = ActivatorConverter.kerasHeadActivation (HeadFunction.Softmax)
                let loss = LossConverter.keras loss
                
                (p, loss, activation.Set [| layer |]), inputs)

        let weights, losses, layers = Array.unzip3 layers
        let optimizer = OptimizerConverter.keras (network.Optimizer)
        let model = new Keras.Models.Model(List.toArray inputs, layers) :> BaseModel

        let args = new Dictionary<string, obj>()
        args.["optimizer"] <- StringOrInstance.op_Implicit optimizer
        args.["loss"] <- losses
        args.["metrics"] <- [|"accuracy"|]
        args.["loss_weights"] <- weights
        args.["sample_weight_mode"] <- null
        args.["weighted_metrics"] <- null
        args.["target_tensors"] <- null
        do model.InvokeMethod("compile", args) |> ignore
        model, AmbiguityConverter.keras network