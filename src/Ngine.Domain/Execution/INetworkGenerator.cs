namespace Ngine.Domain.Execution
{
    public interface INetworkGenerator
    {
        INetwork GenerateFromSchema(Schemas.Network definition);
    }
}
