using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Ngine.Backend.FFI;
using Ngine.CommandLine.Options;
using Ngine.Domain.Execution;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ngine.CommandLine.Command
{
    [Command("train")]
    class TrainCommand
    {
        private readonly INetworkGenerator generator;

        public TrainCommand(INetworkGenerator generator)
        {
            this.generator = generator;
        }

        [FileExists]
        [Argument(0)]
        [FileExtensions(Extensions = "h5")]
        private string ModelPath { get; }

        [FileExists]
        [Argument(1, "-m|--mappings")]
        [FileExtensions(Extensions = "yaml")]
        private string MappingsPath { get; }

        [Argument(2, "-e|--epochs")]
        private uint Epochs { get; }

        [Argument(3, "-b|--batch")]
        private uint Batch { get; }

        [Argument(4, "-vs|--validation-split")]
        private double ValidationSplit { get; }

        /// <summary>
        /// Hanldes command execution.
        /// </summary>
        public async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var network = generator.Instantiate(Path.GetFullPath(ModelPath));
            await network.Train(Path.GetFullPath(MappingsPath), Batch, Epochs, ValidationSplit, cancellationToken);
        }
    }
}
