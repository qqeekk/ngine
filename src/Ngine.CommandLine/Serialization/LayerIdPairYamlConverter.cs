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
    using LayerIdPair = Tuple<LayerId, FSharpOption<LayerId>>;

    class LayerIdPairYamlConverter : IYamlTypeConverter
    {
        private readonly PrimitiveEncoder<LayerId, FSharpList<Errors.ValueOutOfRangeInfo>> encoder;
        private readonly Regex regex;

        public LayerIdPairYamlConverter()
        {
            this.encoder = LayerIdEncoder.encoder;
            this.regex = new Regex($"{encoder.regex.Invoke("fst")}(/{encoder.regex.Invoke("snd")})?");
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
                var pApplied = encoder.decode.Invoke(match.Groups).Invoke(0u);
                var layerIdResult = pApplied.Invoke("fst");
                var prevLayerIdResult = pApplied.Invoke("snd");

                if (layerIdResult.IsOk && OptionModule.IsSome(layerIdResult.ResultValue) && prevLayerIdResult.IsOk)
                {
                    return Tuple.Create(layerIdResult.ResultValue.Value, prevLayerIdResult.ResultValue);
                }
            }

            return null;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var layerIdPair = (LayerIdPair)value;
            var layerId = encoder.encode.Invoke(layerIdPair.Item1);
            
            if (layerIdPair.Item2 != null)
            {
                layerId += $":{encoder.encode.Invoke(layerIdPair.Item2.Value)}";
            }

            emitter.Emit(new Scalar(null, null, layerId, ScalarStyle.Any, true, false));
        }
    }
}