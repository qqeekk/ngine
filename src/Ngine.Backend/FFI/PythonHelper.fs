namespace Ngine.Backend.FFI

module internal PythonHelper =

    open Python.Runtime
    open System

    let activate envPath =
        if not (PythonEngine.IsInitialized) then
            let path' =
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
                |> sprintf "%s\\Scripts;%s" envPath

            do Environment.SetEnvironmentVariable("PATH", path', EnvironmentVariableTarget.Process)

            // Expand PYTHONPATH data which is missing out some directories.
            PythonEngine.PythonPath <- PythonEngine.PythonPath
                + String.Format(";{0};{0}\\Lib;{0}\\Lib\\site-packages", envPath);

            PythonEngine.Initialize()
