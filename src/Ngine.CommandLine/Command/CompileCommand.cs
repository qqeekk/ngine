using McMaster.Extensions.CommandLineUtils;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Services;
using Python.Runtime;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ngine.CommandLine.Command
{
    [Command("compile")]
    internal class CompileCommand
    {
        private readonly ISerializer serializer;
        private readonly INetworkIO<Network> networkReader;
        private readonly INetworkGenerator generator;

        public CompileCommand(ISerializer serializer,
                              INetworkIO<Network> networkReader,
                              INetworkGenerator generator)
        {
            this.serializer = serializer;
            this.networkReader = networkReader;
            this.generator = generator;
        }

        [FileExists]
        [Argument(0)]
        [FileExtensions(Extensions = "yaml")]
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
            if (!networkReader.Read(FileName, out var network))
            {
                return;
            }

            try
            {
                if (Print)
                {
                    Console.WriteLine(network);
                }

                if (!CompileOnly)
                {
                    var (model, ambiguities) = generator.SaveModel(network);
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
            catch (PythonException ex)
            {
                Console.WriteLine($"Internal conversion error: {ex.Message}");
            }
        }
    }
}
