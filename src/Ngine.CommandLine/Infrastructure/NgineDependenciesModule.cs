using Autofac;
using Microsoft.Extensions.Options;
using Ngine.Backend;
using Ngine.Backend.Converters;
using Ngine.CommandLine.Options;
using Ngine.CommandLine.Serialization;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ngine.CommandLine.Infrastructure
{
    class NgineDependenciesModule : Module
    {
        /// <summary>
        /// Register Application-based types to container builder.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var deserializer = new DeserializerBuilder()
                .WithTypeConverter(new LayerIdPairYamlConverter())
                .WithTypeConverter(new LayerIdYamlConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            builder.RegisterInstance(deserializer);
            builder.RegisterInstance(LossConverter.instance);
            builder.RegisterInstance(ActivatorConverter.instance);
            builder.RegisterInstance(OptimizerConverter.instance);
            builder.RegisterInstance(AmbiguityConverter.instance);
            builder.Register(c => KernelConverter.create(c.Resolve<IActivatorConverter>()));
            builder.Register(c => NetworkConverters.create(c.Resolve<ILayerPropsConverter>(), c.Resolve<ILossConverter>(), c.Resolve<IOptimizerConverter>(), c.Resolve<IAmbiguityConverter>()));
            builder.Register(c => new KerasNetworkGenerator(c.Resolve<IOptions<AppSettings>>().Value.ExecutionOptions)).As<INetworkGenerator>();
        }
    }
}
