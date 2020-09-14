using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Ngine.CommandLine.Options;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Abstractions.Services;
using Python.Runtime;
using System;
using System.Threading.Tasks;

namespace Ngine.CommandLine.Command
{
    [Command("compile")]
    internal class CompileCommand
    {
        private readonly INetworkIO<Network> networkReader;
        private readonly INetworkCompiler networkCompiler;
        private readonly string kerasOutputDirectory;

        public CompileCommand(INetworkIO<Network> networkReader,
                              INetworkCompiler networkCompiler,
                              IOptions<AppSettings> options)
        {
            this.networkReader = networkReader;
            this.networkCompiler = networkCompiler;
            this.kerasOutputDirectory = options.Value.ExecutionOptions.OutputDirectory;
        }

        [FileExists]
        [Argument(0)]
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
                    networkCompiler.Write(kerasOutputDirectory, network);
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine($"Ошибка конвертации Keras: {ex.Message}");
            }
        }
    }
}
