using Microsoft.FSharp.Core;

namespace Ngine.Infrastructure.Abstractions.Services
{
    public interface INetworkCompilerOutput
    {
        string CompiledNetworkPath { get; }

        FSharpOption<string> AmbiguitiesPath { get; }
    }
}