using Ngine.Core.Activators.CodeGen;
using System;

namespace Ngine.Core
{
    public interface ILanguageLocatorBuilder
    {
        ILanguageLocatorBuilder AddLanguage(string language, Func<IActivatorCodeGeneratorBuilder, LanguageContext> configure);
        
        ILanguageLocator Build();
    }
}
