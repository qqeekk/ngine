namespace Ngine.Core.Network
{
    public interface INetwork
    {
        void Train(double[] inputs, double[] expected);

        double[] Ask(double[] inputs);
    }
}
