using McMaster.Extensions.CommandLineUtils;
using Ngine.Domain.Execution;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ngine.CommandLine.Command
{
    [Command("tune")]
    public class TuneCommand
    {
        private readonly INetworkGenerator generator;

        public TuneCommand(INetworkGenerator generator)
        {
            this.generator = generator;
        }

        [FileExists]
        [Argument(0)]
        [FileExtensions(Extensions = "h5")]
        private string ModelPath { get; }

        [FileExists]
        [Argument(1, "-a|--ambiguities")]
        [FileExtensions(Extensions = "yaml")]
        private string AmbiguitiesPath { get; }

        [FileExists]
        [Argument(2, "-m|--mappings")]
        [FileExtensions(Extensions = "yaml")]
        private string MappingsPath { get; }

        [Argument(3, "-e|--epochs")]
        private uint Epochs { get; }


        [Argument(4, "-t|--trials")]
        private uint Trials { get; }

        [Argument(5, "-vs|--validation-split")]
        private double ValidationSplit { get; }

        /// <summary>
        /// Hanldes command execution.
        /// </summary>
        public async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var network = generator.Instantiate(Path.GetFullPath(ModelPath));
            await network.Tune(Path.GetFullPath(AmbiguitiesPath), Path.GetFullPath(MappingsPath), Epochs, Trials, ValidationSplit, cancellationToken);
        }
    }
}
