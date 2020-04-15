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

[<CLIMutable>]
type KerasExecutionOptions = {
    OutputDirectory: string
    PythonPath: string
}

/// <summary>
/// Keras network model generator.
/// </summary>
type KerasNetworkGenerator(settings: KerasExecutionOptions) =
    do PythonHelper.activate (settings.PythonPath)

    let saveToFile (kerasModel:BaseModel) =
        // Save keras model to file with random name.
        let randomSuffix = Guid.NewGuid().ToString().[..3]
        let dateString = DateTime.Now.ToString("yyyyMMdd-hh-mm-ss")

        let fileName = sprintf "model-%s.%s.json" dateString randomSuffix
        let filePath = Path.Combine(settings.OutputDirectory, fileName)

        let json = kerasModel.ToJson()
        
        //do  kerasModel.Save(filePath)
        
        // TODO: add exception handling
        do File.WriteAllText(filePath, json)
        filePath

    let generate (NotNull "schema" definition) =
        // Generate model than save immediately
        let filePath = NetworkConverter.keras definition |> saveToFile

        // TODO: Configure python to work with file.
        let predict (NotNull "inputs" inputs) =
            kerasModel.Predict(np.array inputs).GetData<_>()
            
        let train (NotNull "inputs" inputs) (NotNull "expected" expected) =
            do kerasModel.TrainOnBatch(np.array inputs, np.array expected) |> ignore

        { new INetwork with
            member _.Predict inputs = predict inputs
            member _.Train inputs expected = train inputs expected }
                
    interface INetworkGenerator with
        member _.GenerateFromSchema definition = generate definition
