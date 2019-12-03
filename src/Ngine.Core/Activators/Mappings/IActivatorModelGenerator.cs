namespace Ngine.Core.Activators.Mappings
{
    public interface IActivatorModelGenerator
    {
        IActivator GenerateFromSchema(string schema);
    }
}
