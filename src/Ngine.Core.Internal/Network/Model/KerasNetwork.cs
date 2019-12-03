using Keras.Models;
using Ngine.Core.Network;

namespace Ngine.Core.Internal.Network.Model
{
    internal class KerasNetwork : INetwork
    {
        private readonly Sequential model;

        public KerasNetwork(Sequential model)
        {
            this.model = model;
        }

        public double[] Ask(double[] inputs)
        {
            return model.Predict(inputs).GetData<double>();
        }

        public void Train(double[] inputs, double[] expected)
        {
            model.TrainOnBatch(inputs, expected);
        }

        public override string ToString()
        {
            return model.ToString();
        }
    }
}
