using Ngine.Core.Network.CodeGen;

namespace Ngine.Core.Activators.CodeGen
{
    public interface IActivatorCodeGeneratorBuilder
    {
        IActivatorCodeGeneratorBuilder AddCodeGenerator<T>() where T : IActivatorCodeGenerator;
        
        LanguageContext WithNetworkGenerator<T>() where T : INetworkCodeGenerator;
    }
}
