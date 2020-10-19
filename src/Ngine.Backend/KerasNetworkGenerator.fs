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
open System.Reflection

/// <summary>
/// Keras network model generator.
/// </summary>
type KerasNetworkGenerator(pyPath: string) =
    let pythonPath =
        if Path.IsPathFullyQualified pyPath then
            pyPath
        else
            let dir = Directory.GetCurrentDirectory()
            Path.GetFullPath(Path.Combine(dir, pyPath))

    do Console.WriteLine(pythonPath)
    do PythonHelper.activate (pythonPath)

    let saveToFile path (kerasModel:BaseModel) =
        // Save keras model to file with random name.
        let randomSuffix = Guid.NewGuid().ToString().[..3]
        let dateString = DateTime.Now.ToString("yyyy-MM-dd-hhmmss")

        let fileName = sprintf "model-%s.%s.h5" dateString randomSuffix
        let filePath = Path.Combine(path, fileName)
        let json = kerasModel.ToJson()
        
        // TODO: add exception handling
        if not (Directory.Exists path) then Directory.CreateDirectory path |> ignore

        do kerasModel.Save(filePath, overwrite=true, include_optimizer=true)
        do File.WriteAllText(Path.ChangeExtension(filePath, "json"), json)
        filePath

    let save (NotNull "path" path) (NotNull "schema" definition) =
        do Console.WriteLine "Запуск сценариев keras.NET..."
        
        // Generate model than save immediately
        let model, ambiguities = NetworkConverter.keras definition
        saveToFile path model, ambiguities

    let instantiate filePath =
        new KerasNetwork(pythonPath, filePath) :> INetwork

    interface INetworkGenerator with
        member _.SaveModel(path, definition) = save path definition
        member _.Instantiate file = instantiate file
