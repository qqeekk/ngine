﻿using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using YamlDotNet.Serialization;
using static Ngine.Backend.Converters.NetworkErrorPrettyPrinter;
using static Ngine.Domain.Schemas.Errors;

namespace Ngine.Infrastructure.Services
{
    public class InconsistentNetworkIO : NetworkIOBase<InconsistentNetwork, InconsistentLayerConversionError>
    {
        public InconsistentNetworkIO(INetworkConverter converter, IDeserializer deserializer, ISerializer serializer)
            : base(converter, deserializer, serializer)
        {
        }

        protected override FSharpResult<InconsistentNetwork, NetworkConversionError<InconsistentLayerConversionError>[]> Decode(Schema.Network schema)
        {
            return NetworkConverter.DecodeInconsistent(schema);
        }

        protected override Schema.Network Encode(InconsistentNetwork network)
        {
            return NetworkConverter.EncodeInconsistent(network);
        }

        protected override PrettyTree[] Prettify(NetworkConversionError<InconsistentLayerConversionError>[] error)
        {
            return prettifyInconsistent(error);
        }
    }
}
