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

    let private unzipScripts target =
        //if not (File.Exists target) then
            let assembly = Assembly.GetExecutingAssembly()
            use resource = assembly.GetManifestResourceStream("Ngine.Backend.output.zip")
            use fileStream = new FileStream(target, FileMode.Create, FileAccess.ReadWrite)
            resource.CopyTo fileStream

    let activate (envPath: string) =
        if not (PythonEngine.IsInitialized) then
            do unzipScripts "output.zip"

            let path' =
                let path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
                String.Format("{0}\\Scripts;{0};{1}", envPath, path)

            do Environment.SetEnvironmentVariable("PATH", path', EnvironmentVariableTarget.Process)

            // Expand PYTHONPATH data which is missing out some directories.
            PythonEngine.PythonPath <- PythonEngine.PythonPath
                + String.Format(";{0};{0}\\Lib;{0}\\Lib\\site-packages", envPath);

            Keras.Setup.UseTfKeras()
            PythonEngine.Initialize()

    let execute (token:CancellationToken) path args =
        let python = Path.Combine(path, "Scripts", "python")

        Task.Run(fun() ->
            //let runCommand = sprintf 
            let error = new StringBuilder()

            let s = ProcessStartInfo(python, sprintf "output.zip %s" args)
            s.RedirectStandardOutput <- true
            s.RedirectStandardError <- true
            s.RedirectStandardInput <- true
            s.UseShellExecute <- false
            s.CreateNoWindow <- true

            use p = new Process()
            p.StartInfo <- s
            p.OutputDataReceived.Add(fun a -> if a.Data <> null then Console.WriteLine a.Data)
            p.ErrorDataReceived.Add(fun a -> error.AppendLine(a.Data) |> ignore)
        
            let _ = p.Start()
            p.BeginOutputReadLine()
            p.BeginErrorReadLine()
            use r = token.Register(Action(p.Kill))

            // print error
            p.WaitForExit()
            Console.WriteLine(string error))
