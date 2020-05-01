using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Ngine.Backend;
using Ngine.Backend.Converters;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static Ngine.Backend.Converters.NetworkErrorPrettyPrinter;

namespace Ngine.CommandLine.Command
{
    [Command("compile")]
    internal class CompileCommand
    {
        private readonly IDeserializer deserializer;
        private readonly ISerializer serializer;
        private readonly INetworkConverter converter;
        private readonly INetworkGenerator generator;

        public CompileCommand(IDeserializer deserializer,
                              ISerializer serializer,
                              INetworkConverter converter,
                              INetworkGenerator generator)
        {
            this.deserializer = deserializer;
            this.serializer = serializer;
            this.converter = converter;
            this.generator = generator;
        }

        [FileExists]
        [Argument(0)]
        [FileExtensions(Extensions ="yaml")]
        private string FileName { get; }

        [Option("-p|--print")]
        private bool Print { get; }

        [Option("--compile-only")]
        private bool CompileOnly { get; }


        /// <summary>
        /// Hanldes command execution.
        /// </summary>
        public async Task OnExecuteAsync()
        {
            using var file = File.OpenText(FileName);
            
            try
            {
                var obj = deserializer.Deserialize<Schema.Network>(file);

                var network = converter.Decode(obj);
                if (network.IsOk)
                {
                    var result = network.ResultValue;
                    Console.WriteLine("Parsing successful!");

                    if (Print)
                    {
                        Console.WriteLine(result);
                    }

                    if (!CompileOnly)
                    {
                        var (model, ambiguities) = generator.SaveModel(result);
                        Console.WriteLine("Model saved to file {0}", model);
                        
                        if (ambiguities.Ambiguities.Length > 0)
                        {
                            var ambiguitiesYaml = serializer.Serialize(ambiguities);
                            var ambiguitiesPath = Path.ChangeExtension(model, "ambiguities.yaml");
                            File.WriteAllText(ambiguitiesPath, ambiguitiesYaml);

                            Console.WriteLine("Ambiguities ({0}) saved to file {1}", ambiguities.Ambiguities.Length, ambiguitiesPath);
                        }
                    }
                }
                else
                {
                    var error = prettify(network.ErrorValue);

                    Console.WriteLine("Network definition is invalid - {0} errors total.", error.Length);
                    Array.ForEach(error, r => PrintPrettyTree(r));
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine($"Internal conversion error: {ex.Message}");
            }
            catch (YamlException ex)
            {
                Console.WriteLine($"Error while parsing network definition: {ex.Message}");
            }
        }

        private void PrintPrettyTree(PrettyTree pretty, int indents = 0)
        {
            var indent = new string(' ', 3 * indents);
            
            Console.WriteLine(indent + $"-> {pretty.Item1}:");

            if (!pretty.Item2.Any())
            {
                Console.WriteLine();
            }

            foreach (var dep in pretty.Item2)
            {
                PrintPrettyTree(dep, indents + 1);
            }
        }
    }
}
