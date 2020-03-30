namespace Ngine.Domain.CodeGen
{
    public interface INetworkCodeGenerator
    {
        string GenerateFromDefinition(Schemas.Network definition);
    }
}
