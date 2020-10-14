namespace Ngine.Backend

open Ngine.Domain.Execution
open Ngine.Backend.FFI
open System.IO
open FSharp.Control.Tasks.V2
open System

type KerasNetwork(pythonPath: string, modelPath: string) =
    do PythonHelper.activate pythonPath
    
    interface INetwork with
        member _.Tune (ambiguitiesFile: string) (mappings: string) (trials:uint32) (epochs: uint32) (validationSplit: float) token = 
            task {
                let randomSuffix = Guid.NewGuid().ToString().[..3]
                let dateString = DateTime.Now.ToString("yyyy-MM-dd-hhmmss")

                let ambiguitiesFileExtension = Path.GetExtension ambiguitiesFile
                let resolveAmbiguitiesPath = Path.ChangeExtension(ambiguitiesFile, sprintf "resolved-%s.%s.%s" randomSuffix dateString ambiguitiesFileExtension)
                
                do! (PythonHelper.execute token pythonPath <| sprintf "tune \"%s\" \"%s\" \"%s\" %d %d %f %s" 
                    modelPath ambiguitiesFile mappings (int trials) (int epochs) validationSplit resolveAmbiguitiesPath)
                
                return resolveAmbiguitiesPath
            }

        member _.Train mappings batch epochs validationSplit token = 
            PythonHelper.execute token pythonPath <| sprintf "train \"%s\" \"%s\" %d %d %f" 
                modelPath mappings (int batch) (int epochs) validationSplit
