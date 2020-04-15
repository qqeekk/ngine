using Keras.Layers;
using Ngine.Backend;
using Ngine.Backend.Converters;
using Ngine.CommandLine.Options;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Python.Runtime;
using System;
using System.Diagnostics;
using System.IO;

namespace Ngine.CommandLine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var settings = new KerasExecutionOptions
            {
                PythonPath = @"D:\projects\diploma\Ngine\src\Ngine.Backend.Python\env",
                OutputDirectory = Path.Combine(Directory.GetCurrentDirectory() , "models"),
            };
            
            Directory.CreateDirectory(settings.OutputDirectory);

            var schema = new Schema.Network
            {
                Layers = new[]
                {
                    new Schema.Layer
                    {
                        Neurons = "100",
                        Activator = "sigmoid",
                        Type = "dense",
                    },
                    new Schema.Layer
                    {
                        Neurons = "10",
                        Activator = "relu",
                        Type = "dense",

                    }
                }
            };

            var networkConverter = NetworkConverters.create(ActivatorConverter.instance, KernelConverter.instance);
            var network = networkConverter.Decode(schema).ResultValue;
            //new Keras.Shape
            //new C()

            var generator = new KerasNetworkGenerator(settings) as INetworkGenerator;
            generator.GenerateFromSchema(network);
        }
    }
}
