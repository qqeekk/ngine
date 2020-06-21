using Autofac;
using Microsoft.Extensions.Options;
using Ngine.Backend;
using Ngine.Backend.Converters;
using Ngine.CommandLine.Options;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Ngine.Domain.Services.Conversion;
using Ngine.Infrastructure.Serialization;
using Ngine.Infrastructure.Services;

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
            builder.RegisterInstance(SerializationProfile.Deserializer);
            builder.RegisterInstance(SerializationProfile.Serializer);
            builder.RegisterInstance(LossConverter.instance);
            builder.RegisterInstance(ActivatorConverter.instance);
            builder.RegisterInstance(OptimizerConverter.instance);
            builder.RegisterInstance(AmbiguityConverter.instance);
            builder.Register(c => KernelConverter.create(c.Resolve<IActivatorConverter>()));
            builder.Register(c => NetworkConverters.create(c.Resolve<ILayerPropsConverter>(), c.Resolve<ILossConverter>(), c.Resolve<IOptimizerConverter>(), c.Resolve<IAmbiguityConverter>()));
            builder.Register(c => new KerasNetworkGenerator(c.Resolve<IOptions<AppSettings>>().Value.ExecutionOptions)).As<INetworkGenerator>();
            
            builder.RegisterType<NetworkIO>().As<INetworkIO<Network>>();
        }
    }
}
