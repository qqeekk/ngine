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
        private readonly INetworkIO<Network> networkReader;
        private readonly KerasNetworkIO kerasNetworkIO;

        public CompileCommand(INetworkIO<Network> networkReader,
                              KerasNetworkIO kerasNetworkIO)
        {
            this.networkReader = networkReader;
            this.kerasNetworkIO = kerasNetworkIO;
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
                    kerasNetworkIO.Write(network);
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine($"Ошибка конвертации Keras: {ex.Message}");
            }
        }
    }
}
