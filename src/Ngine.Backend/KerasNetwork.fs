namespace Ngine.Backend

open Ngine.Domain.Execution
open Ngine.Backend.FFI

[<CLIMutable>]
type KerasExecutionOptions = {
    OutputDirectory: string
    PythonPath: string
}

type KerasNetwork(settings: KerasExecutionOptions, modelPath: string) =
    do PythonHelper.activate settings.PythonPath
    
    interface INetwork with
        member _.Train mappings batch epochs validationSplit token = 
            PythonHelper.execute token <| sprintf "train \"%s\" \"%s\" %d %d %f" 
                modelPath mappings (int batch) (int epochs) validationSplit
        
        member _.Predict mappings weights token =
            PythonHelper.execute token <| sprintf "predict \"%s\" \"%s\" \"%s\"" 
                modelPath mappings weights
                
