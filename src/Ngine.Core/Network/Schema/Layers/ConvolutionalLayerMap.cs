namespace Ngine.Core.Network.Schema.Layers
{
    public class ConvolutionalLayerMap : ILayerMap
    {
        public ConvolutionalLayerMap(int neurons, int mapWidth, int mapHeight)
        {
            NeuronsTotal = neurons;
            MapWidth = mapWidth;
            MapHeight = mapHeight;
        }

        public int MapWidth { get; }

        public int MapHeight { get; }

        public int NeuronsTotal { get; }
    }
}