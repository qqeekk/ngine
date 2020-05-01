namespace Ngine.Backend
open Ngine.Domain.Execution
open Ngine.Domain.Schemas
open Ngine.Domain.Utils
open Ngine.Domain.Schemas.Expressions
open Keras.Models
open Keras.Layers
open Numpy
open System.IO
open System
open Ngine.Backend.Converters
open Ngine.Backend.FFI
open Keras

/// <summary>
/// Keras network model generator.
/// </summary>
type KerasNetworkGenerator(settings: KerasExecutionOptions) =
    do PythonHelper.activate (settings.PythonPath)

    let saveToFile (kerasModel:BaseModel) =
        // Save keras model to file with random name.
        let randomSuffix = Guid.NewGuid().ToString().[..3]
        let dateString = DateTime.Now.ToString("yyyy-MM-dd-hhmmss")

        let fileName = sprintf "model-%s.%s.h5" dateString randomSuffix
        let filePath = Path.Combine(settings.OutputDirectory, fileName)
        let json = kerasModel.ToJson()
        
        // TODO: add exception handling
        do kerasModel.Save(filePath, overwrite=true, include_optimizer=true)
        do File.WriteAllText(Path.ChangeExtension(filePath, "json"), json)
        filePath

    let save (NotNull "schema" definition) =
        // TODO: replace with log
        do printfn "Start conversion using keras.NET..."
        
        // Generate model than save immediately
        let model, ambiguities = NetworkConverter.keras definition
        saveToFile model, ambiguities

    let instantiate filePath =
        new KerasNetwork(settings, filePath) :> INetwork

    interface INetworkGenerator with
        member _.SaveModel definition = save definition
        member _.Instantiate file = instantiate file
