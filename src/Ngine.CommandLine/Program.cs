using Ngine.CommandLine.Options;
using Numpy;
using Python.Runtime;
using System;
using System.IO;

namespace Ngine.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new AppSettings
            {
                PathToPythonInterpreter = Path.Combine(Directory.GetCurrentDirectory(), "venv/python.exe")
            };

            PythonEngine.PythonPath = settings.PathToPythonInterpreter;

            Console.WriteLine("Hello World!");
        }
    }
}
