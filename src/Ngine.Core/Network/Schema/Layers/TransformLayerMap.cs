namespace Ngine.Core.Network.Schema.Layers
{
    public class TransformLayerMap : ILayerMap
    {
        public TransformLayerMap(int count)
        {
            NeuronsTotal = count;
        }

        public int NeuronsTotal { get; }
    }
}