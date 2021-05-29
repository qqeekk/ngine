using Microsoft.FSharp.Core;
using Ngine.Infrastructure.Abstractions.Services;

namespace Ngine.Infrastructure.Services
{
    internal class KerasNetworkCompilerOutput : INetworkCompilerOutput
    {
        public string CompiledNetworkPath { get; set; } = string.Empty;
        public FSharpOption<string> AmbiguitiesPath { get; set; } = FSharpOption<string>.None;
    }
}
