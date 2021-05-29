using McMaster.Extensions.CommandLineUtils;
using Ngine.CommandLine.Command;
using System.Threading.Tasks;

namespace Ngine.CommandLine
{
    [Subcommand(typeof(TrainCommand))]
    [Subcommand(typeof(ListCommand))]
    [Subcommand(typeof(CompileCommand))]
    [Subcommand(typeof(TuneCommand))]
    internal class NgineApplication
    {
        /// <summary>
        /// Hanldes command execution.
        /// </summary>
        public async Task OnExecuteAsync()
        {

        }
    }
}