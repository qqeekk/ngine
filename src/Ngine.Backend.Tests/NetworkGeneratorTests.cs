using Ngine.Backend;
using Ngine.Domain.Schemas;
using Ngine.Domain.Schemas.Conversions;
using Ngine.Domain.Schemas.Expressions;
using Ngine.Domain.Schemas.Kernels;
using NUnit.Framework;

namespace Ngine.Core.Internal.Tests
{
    public class Tests
    {
        [Test]
        public void Test1()
        {
            var network = new Network
            {
                Layers = new[]
                {
                    new Layer
                    {
                        Activator = Activator.NewQuotedFunction(QuotedFunction.Sigmoid),
                        Kernel = Kernel.Dense,
                        NeuronsTotal = 100,
                    },
                    new Layer
                    {
                        Activator = Activator.NewQuotedFunction(QuotedFunction.Sigmoid),
                        Kernel = Kernel.NewConv2D(new Convolutional2DKernel(5,6)),
                        NeuronsTotal = 30,
                    },
                }
            };

            var schema = Networks.encode(network);
            var network1 = Networks.decode(schema); 
        }

        [Test]
        public void SchemaErrorTest()
        {
            var schema = new Raw.Network
            {
                Layers = new[]
                {
                    new Raw.Layer
                    {
                        Type = "dense",
                        Neurons = "1000000000000000000000000000000",
                        Activator = "sigmod",
                    },
                    new Raw.Layer
                    {
                        Type = "dens",
                        Neurons = "10:[10x10]",
                        Activator = "sigmoid",
                    },
                    new Raw.Layer
                    { 
                        Type = "conv2D",
                        Neurons = "10[10x10]",
                        Activator = "sigmoid",
                    },
                }
            };

            var network = Networks.decode(schema);
        }
    } 
}