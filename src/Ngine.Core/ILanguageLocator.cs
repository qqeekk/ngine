namespace Ngine.Core
{
    public interface ILanguageLocator
    {
        LanguageContext ResolveFor(string language);
    }
}