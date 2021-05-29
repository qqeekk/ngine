using Ngine.Domain.Schemas;

namespace Ngine.Infrastructure.Abstractions.Services
{
    public interface INetworkIO<TNetwork>
    {
        IFileFormat FileFormat { get; }

        INetworkConverter NetworkConverter { get; }

        bool TryParse(Schema.Network network, out TNetwork result);

        bool Read(string fileName, out TNetwork result);

        void Write(string fileName, TNetwork network);
    }
}