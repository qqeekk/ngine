namespace Ngine.Core.Activators.Mappings
{
    public interface IActivatorModelGeneratorBuilder
    {
        IActivatorModelGeneratorBuilder AddModelGenerator<T>() where T : IActivatorModelGenerator;

        IActivatorModelGenerator Build();
    }
}
