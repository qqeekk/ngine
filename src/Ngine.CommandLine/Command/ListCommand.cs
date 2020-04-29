using McMaster.Extensions.CommandLineUtils;
using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Ngine.Domain.Schemas.Errors;

namespace Ngine.CommandLine
{
    [Command("list")]
    internal class ListCommand
    {
        private readonly IActivatorConverter activatorConverter;
        private readonly ILossConverter lossConverter;
        private readonly ILayerPropsConverter propsConverter;
        private readonly IOptimizerConverter optimizerConverter;
        private readonly IAmbiguityConverter ambiguityConverter;

        public ListCommand(IActivatorConverter activatorConverter,
                           ILossConverter lossConverter,
                           ILayerPropsConverter propsConverter,
                           IOptimizerConverter optimizerConverter,
                           IAmbiguityConverter ambiguityConverter)
        {
            this.activatorConverter = activatorConverter ?? throw new System.ArgumentNullException(nameof(activatorConverter));
            this.lossConverter = lossConverter ?? throw new System.ArgumentNullException(nameof(lossConverter));
            this.propsConverter = propsConverter ?? throw new System.ArgumentNullException(nameof(propsConverter));
            this.optimizerConverter = optimizerConverter ?? throw new System.ArgumentNullException(nameof(optimizerConverter));
            this.ambiguityConverter = ambiguityConverter;
        }
        private enum ListProperties
        {
            Layers,
            Activations,
            HeadActivations,
            Losses,
            Optimizers,
            Ambiguities
        }

        [Argument(0)]
        private ListProperties Name { get; }

        [Option("-r|--with-regex")]
        private bool WithRegex { get; }

        /// <summary>
        /// Hanldes command execution.
        /// </summary>
        public async Task OnExecuteAsync()
        {
            var pretties = Name switch
            {
                ListProperties.Layers => propsConverter.LayerTypeNames,
                ListProperties.Activations => activatorConverter.ActivationFunctionNames,
                ListProperties.HeadActivations => activatorConverter.HeadFunctionNames,
                ListProperties.Losses => lossConverter.LossFunctionNames,
                ListProperties.Optimizers => optimizerConverter.OptimizerNames,
                ListProperties.Ambiguities => new[] { ambiguityConverter.ListPattern }
            };

            foreach (var pretty in pretties)
            {
                PrintPretty(pretty);
            }
        }

        private void PrintPretty(Pretty pretty, int indents = 0)
        {
            var indent = new string(' ', 3 * indents);
            if (WithRegex)
            {
                Console.WriteLine(indent + $"~~ {pretty.name}: '{pretty.regex}'");
                return;
            }
            
            Console.WriteLine(indent + $"-> {pretty.name}: '{OptionModule.DefaultValue("", pretty.defn)}'");

            if (!pretty.deps.Any())
            {
                Console.WriteLine();
            }

            foreach (var dep in pretty.deps)
            {
                PrintPretty(dep, indents + 1);
            }
        }
    }
}