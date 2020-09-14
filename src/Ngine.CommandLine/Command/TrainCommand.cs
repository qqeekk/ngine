using McMaster.Extensions.CommandLineUtils;
using Ngine.Domain.Execution;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
