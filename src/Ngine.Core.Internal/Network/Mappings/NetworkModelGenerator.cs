using Keras;
using Keras.Layers;
using Keras.Models;
using Ngine.Core.Internal.Network.Model;
using Ngine.Core.Network;
using Ngine.Core.Network.Mappings;
using Ngine.Core.Network.Schema;
using Ngine.Core.Network.Schema.Layers;
using System;
using System.Linq;

namespace Ngine.Core.Internal.Network.Mappings
{
    internal class NetworkModelGenerator : INetworkModelGenerator
    {
        public INetwork GenerateFromSchema(NetworkDefinition definition)
        {
            var layers = Array.ConvertAll(definition.Layers, GenerateKerasLayerFromSchema);
            var model = new Sequential(layers);
            return new KerasNetwork(model);
        }

        private static BaseLayer GenerateKerasLayerFromSchema(NetworkLayerDefinition layer)
        {
            switch (layer.Type)
            {
                case NetworkLayerType.Convolutional:
                    var clp = layer.Neurons as ConvolutionalLayerMap;
                    return new Conv2D(
                        filters: layer.Neurons.NeuronsTotal, 
                        kernel_size: Tuple.Create(clp.MapWidth, clp.MapHeight), 
                        activation: layer.Activator);

                case NetworkLayerType.SensorOrTrasform:
                    return new Dense(layer.Neurons.NeuronsTotal, 
                        activation: layer.Activator);

                default:
                    return null;
            };
        }
    }
}