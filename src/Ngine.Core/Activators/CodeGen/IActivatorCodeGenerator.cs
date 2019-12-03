namespace Ngine.Core.Activators.CodeGen
{
    public interface IActivatorCodeGenerator
    {
        ActivatorCodeFragment GenerateFromSchema(string schema);
    }
}
