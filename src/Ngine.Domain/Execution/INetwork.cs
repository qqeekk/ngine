namespace Ngine.Domain.Execution
{
    public interface INetwork
    {
        void Train(double[] inputs, double[] expected);

        double[] Ask(double[] inputs);
    }
}
