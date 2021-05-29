using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;

namespace NgineUI.ViewModels.Network.Connections
{
    public class NgineInputViewModel<T> : ValueNodeInputViewModel<T>
    {
        public NgineInputViewModel(PortType type)
        {
            this.Port = new NginePortViewModel { PortType = type };
            //this.PortPosition = PortPosition.Right;
        }
    }
}
