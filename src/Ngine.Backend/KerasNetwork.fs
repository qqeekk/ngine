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
        member _.Tune (ambiguitiesFile: string) (mappings: string) (trials:uint32) (epochs: uint32) (validationSplit: float) token =
            PythonHelper.execute token <| sprintf "tune \"%s\" \"%s\" \"%s\" %d %d %f" 
                modelPath ambiguitiesFile mappings (int trials) (int epochs) validationSplit

        member _.Train mappings batch epochs validationSplit token = 
            PythonHelper.execute token <| sprintf "train \"%s\" \"%s\" %d %d %f" 
                modelPath mappings (int batch) (int epochs) validationSplit
