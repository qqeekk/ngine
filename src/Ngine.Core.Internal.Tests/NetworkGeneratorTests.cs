using Ngine.Core.Internal.Network.Mappings;
using Ngine.Core.Internal.Network.Model;
using Ngine.Core.Network.Schema;
using Ngine.Core.Network.Schema.Layers;
using NUnit.Framework;

namespace Ngine.Core.Internal.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var networkDefinition = new NetworkDefinition
            {
                Layers = new[]
                {
                    new NetworkLayerDefinition
                    {
                        Activator = "sigmoid",
                        Neurons = new TransformLayerMap(100),
                        Type = NetworkLayerType.SensorOrTrasform,
                    },
                    new NetworkLayerDefinition
                    {
                        Activator = "sigmoid",
                        Neurons = new ConvolutionalLayerMap(5,6,30),
                        Type = NetworkLayerType.Convolutional,
                    },
                }
            };

            var network = new NetworkModelGenerator().GenerateFromSchema(networkDefinition) as KerasNetwork;
            var py_str = network.ToString();
        }
    }
}