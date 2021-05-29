using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;

namespace Ngine.Infrastructure.Abstractions.Services
{
    public interface INetworkCompiler
    {
        INetworkGenerator NetworkGenerator { get; }

        INetworkCompilerOutput Write(string folderName, Network network);
    }
}