using Ngine.Domain.Schemas;

namespace Ngine.Infrastructure.Services
{
    public interface INetworkIO<TNetwork>
    {
        INetworkConverter NetworkConverter { get; }

        bool Read(string fileName, out TNetwork result);

        void Write(string fileName, TNetwork network);
    }
}