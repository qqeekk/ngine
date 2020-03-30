namespace Ngine.Domain.CodeGen
{
    public interface ILanguageLocatorBuilder
    {
        ILanguageLocatorBuilder AddLanguage(string language, INetworkCodeGenerator generator);
        
        ILanguageLocator Build();
    }
}
