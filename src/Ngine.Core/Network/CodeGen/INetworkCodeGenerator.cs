using Ngine.Core.Activators.CodeGen;
using Ngine.Core.Network.Schema;

namespace Ngine.Core.Network.CodeGen
{
    public interface INetworkCodeGenerator
    {
        string GenerateFromSchema(NetworkDefinition definition);
    }
}
