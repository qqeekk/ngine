using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ngine.Infrastructure.Serialization
{
    public static class SerializationProfile
    {
        public static IDeserializer Deserializer { get; } =
            new DeserializerBuilder()
                .WithTypeConverter(new LayerIdPairYamlConverter())
                .WithTypeConverter(new LayerIdYamlConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

        public static ISerializer Serializer { get; } =
            new SerializerBuilder()
                .WithTypeConverter(new LayerIdPairYamlConverter())
                .WithTypeConverter(new LayerIdYamlConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
    }
}
