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

namespace Ngine.CommandLine.Serialization
{
    class LayerIdYamlConverter : IYamlTypeConverter
    {
        private readonly PrimitiveEncoder<LayerId, FSharpList<Errors.ValueOutOfRangeInfo>> encoder;
        private readonly Regex regex;

        public LayerIdYamlConverter()
        {
            this.encoder = LayerIdEncoder.encoder;
            this.regex = new Regex(encoder.regex.Invoke("fst"));
        }

        public bool Accepts(Type type)
        {
            return type == typeof(LayerId);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var value = parser.Consume<Scalar>().Value;
            if (regex.Match(value) is var match && match.Success)
            {
                var layerIdResult = encoder.decode.Invoke(match.Groups).Invoke(0u).Invoke("fst");

                if (layerIdResult.IsOk && OptionModule.IsSome(layerIdResult.ResultValue))
                {
                    return layerIdResult.ResultValue.Value;
                }
            }

            return null;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var layerIdPair = (LayerId)value;
            var layerId = encoder.encode.Invoke(layerIdPair);

            emitter.Emit(new Scalar(null, null, layerId, ScalarStyle.Any, true, false));
        }
    }
}