using System;
using System.Collections.Generic;
using System.Text;

namespace NgineUI.ViewModels.Network.Nodes
{
    public interface IConfigurable<TConfig>
    {
        void Setup(TConfig config);
    }
}
