namespace Ngine.Backend.FFI

open Keras
open System.Reflection
open System.IO
open System.Diagnostics
open System.Threading
open System.Text
open System.Threading.Tasks

/// TODO: make internal
module PythonHelper =
    open Python.Runtime
    open System

    let private lock = obj()

    let private unzipScripts target =
        //if not (File.Exists target) then
            let assembly = Assembly.GetExecutingAssembly()
            use resource = assembly.GetManifestResourceStream("Ngine.Backend.output.zip")
            use fileStream = new FileStream(target, FileMode.Create, FileAccess.ReadWrite)
            resource.CopyTo fileStream

    let activate envPath =
        if not (PythonEngine.IsInitialized) then
            do unzipScripts "output.zip"

            let path' =
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
                |> sprintf "%s\\Scripts;%s" envPath

            do Environment.SetEnvironmentVariable("PATH", path', EnvironmentVariableTarget.Process)

            // Expand PYTHONPATH data which is missing out some directories.
            PythonEngine.PythonPath <- PythonEngine.PythonPath
                + String.Format(";{0};{0}\\Lib;{0}\\Lib\\site-packages", envPath);

            PythonEngine.Initialize()

    let execute (token:CancellationToken) args =
        Task.Run(fun() ->
            use p = new Process()
            p.StartInfo <- ProcessStartInfo("python", sprintf "output.zip %s" args)
            p.StartInfo.RedirectStandardOutput <- true
            p.StartInfo.RedirectStandardError <- true
            p.StartInfo.RedirectStandardInput <- true
            p.StartInfo.UseShellExecute <- false
            p.StartInfo.CreateNoWindow <- true
            
            let error = new StringBuilder()
            p.OutputDataReceived.Add(fun a -> printfn "%s" a.Data)
            p.ErrorDataReceived.Add(fun a -> error.AppendLine(a.Data) |> ignore)
        
            let _ = p.Start()
            p.BeginOutputReadLine()
            p.BeginErrorReadLine()

            use r = token.Register(Action(p.Kill))

            // print error
            p.WaitForExit()
            printfn "\n%s" (string error))
