using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Abstractions.Services;
using Ngine.Infrastructure.Services.FileFormats;
using YamlDotNet.Serialization;
using static Ngine.Backend.Converters.NetworkErrorPrettyPrinter;
using static Ngine.Domain.Schemas.Errors;

namespace Ngine.Infrastructure.Services
{
    public class NetworkIO : NetworkIOBase<Network, LayerConversionError>
    {
        public override IFileFormat FileFormat { get; } = new NgineSchemaFormat();

        public NetworkIO(INetworkConverter converter, IDeserializer deserializer, ISerializer serializer)
            : base(converter, deserializer, serializer)
        {
        }

        protected override FSharpResult<Network, NetworkConversionError<LayerConversionError>[]> Decode(Schema.Network schema)
        {
            return NetworkConverter.Decode(schema);
        }

        protected override Schema.Network Encode(Network network)
        {
            return NetworkConverter.Encode(network);
        }

        protected override PrettyTree[] Prettify(NetworkConversionError<LayerConversionError>[] error)
        {
            return prettify(error);
        }
    }
}
