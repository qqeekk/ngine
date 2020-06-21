using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Ngine.Backend.Converters;
using Ngine.Domain.Schemas;
using System;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using LayerId = System.Tuple<uint, uint>;

namespace Ngine.Infrastructure.Serialization
{
    using LayerIdPair = Tuple<LayerId, FSharpOption<LayerId>>;

    internal class LayerIdPairYamlConverter : IYamlTypeConverter
    {
        private readonly PrimitiveEncoder<LayerIdPair, Errors.PropsConversionError> encoder;
        private readonly Regex regex;

        public LayerIdPairYamlConverter()
        {
            this.encoder = LayerConnectionEncoder.encoder;
            this.regex = new Regex(encoder.regex.Invoke("con"));
        }

        public bool Accepts(Type type)
        {
            return type == typeof(LayerIdPair);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var value = parser.Consume<Scalar>().Value;
            if (regex.Match(value) is var match && match.Success)
            {
                var layerConResult = encoder.decode.Invoke(match.Groups).Invoke(0u).Invoke("con");

                if (layerConResult.IsOk && OptionModule.IsSome(layerConResult.ResultValue))
                {
                    return layerConResult.ResultValue.Value;
                }
            }

            return null;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var layerIdPair = (LayerIdPair)value;
            var layerConnection = encoder.encode.Invoke(layerIdPair);

            emitter.Emit(new Scalar(null, null, layerConnection, ScalarStyle.Any, true, false));
        }
    }
}