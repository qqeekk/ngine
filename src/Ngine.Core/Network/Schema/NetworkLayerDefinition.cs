using System.ComponentModel;

namespace Ngine.Core.Network.Schema
{
    public enum NetworkLayerType
    {
        [Description("sensor/transform")]
        SensorOrTrasform = 1,

        [Description("convolutional")]
        Convolutional,
    }

    public class NetworkLayerDefinition
    {
        public NetworkLayerType Type { get; set; }

        public ILayerMap Neurons { get; set; }

        public string Activator { get; set; }
    }
}