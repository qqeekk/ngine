using Ngine.Core.Activators.CodeGen;
using Ngine.Core.Network.CodeGen;

namespace Ngine.Core
{
    public class LanguageContext
    {
        public LanguageContext(INetworkCodeGenerator networkCodeGenerator, IActivatorCodeGenerator activatorCodeGenerator)
        {
            NetworkCodeGenerator = networkCodeGenerator;
            ActivatorCodeGenerator = activatorCodeGenerator;
        }

        public INetworkCodeGenerator NetworkCodeGenerator { get; }

        public IActivatorCodeGenerator ActivatorCodeGenerator { get; }
    }
}