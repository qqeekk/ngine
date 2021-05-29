using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace NgineUI.ViewModels.Network.Connections
{
    public class NgineOutputViewModel<T> : ValueNodeOutputViewModel<T>
    {
        public NgineOutputViewModel(PortType type)
        {
            this.Port = new NginePortViewModel { PortType = type };
            //this.PortPosition = PortPosition.Left;
        }
    }
}
