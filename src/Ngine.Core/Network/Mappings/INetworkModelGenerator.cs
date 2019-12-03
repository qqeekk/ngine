using Ngine.Core.Network.Schema;

namespace Ngine.Core.Network.Mappings
{
    public interface INetworkModelGenerator
    {
        INetwork GenerateFromSchema(NetworkDefinition definition);
    }
}
