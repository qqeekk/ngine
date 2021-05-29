using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NgineUI.ViewModels.Network.Connections
{
    public enum PortType
    {
        Head,
        Layer1D,
        Layer2D,
        Layer3D,
    }

    public class NginePortViewModel : PortViewModel
    {
        private PortType portType;

        public PortType PortType
        {
            get => portType;
            set => this.RaiseAndSetIfChanged(ref portType, value);
        }
    }
}
