namespace Ngine.Domain.CodeGen
{
    public interface ILanguageLocator
    {
        INetworkCodeGenerator ResolveFor(string language);
    }
}