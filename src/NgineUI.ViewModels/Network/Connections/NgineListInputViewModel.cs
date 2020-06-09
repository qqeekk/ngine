using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;

namespace NgineUI.ViewModels.Network.Connections
{
    public class NgineListInputViewModel<T> : ValueListNodeInputViewModel<T>
    {
        public NgineListInputViewModel(PortType type)
        {
            this.Port = new NginePortViewModel { PortType = type };
            //this.PortPosition = PortPosition.Right;
        }
    }
}
