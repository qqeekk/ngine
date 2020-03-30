namespace Ngine.Backend
open Ngine.Domain.Execution
open Ngine.Domain.Schemas
open Ngine.Domain.Schemas.Kernels
open Ngine.Domain.Schemas.Expressions
open Keras.Models
open Keras.Layers
open Numpy

[<RequireQualifiedAccess>]
module private Projections =
    let mapActivator = function
        | QuotedFunction(Sigmoid) -> "sigmoid"
        | QuotedFunction(ReLu) -> "relu"

    let mapLayer (layerSchema : Layer) =
        let activator = mapActivator (layerSchema.Activator)

        match layerSchema.Kernel with
        | Conv2D conv2d -> 
            new Keras.Layers.Conv2D(
                int layerSchema.NeuronsTotal,
                (int conv2d.Width, int conv2d.Height),
                activation = activator) :> BaseLayer

        | Conv3D conv3d ->
            new Keras.Layers.Conv3D(
                int layerSchema.NeuronsTotal,
                (int conv3d.Width, int conv3d.Height, int conv3d.Depth),
                activation = activator) :> BaseLayer

        | Dense ->
            new Keras.Layers.Dense(
                int layerSchema.NeuronsTotal,
                activation = activator) :> BaseLayer


type KerasNetworkGenerator() =
    interface INetworkGenerator with
        member _.GenerateFromSchema definition =
            let kerasLayers = Array.map (Projections.mapLayer) definition.Layers;
            let kerasModel = new Sequential(kerasLayers) :> BaseModel;
            
            { new INetwork with
                  member _.Ask inputs =
                      kerasModel.Predict(np.array inputs).GetData<_>()

                  member _.Train (inputs, expected) =
                      do kerasModel.TrainOnBatch(np.array inputs, np.array expected) |> ignore
            }

    member this.GenerateFromSchema definition =
        (this :> INetworkGenerator).GenerateFromSchema definition
