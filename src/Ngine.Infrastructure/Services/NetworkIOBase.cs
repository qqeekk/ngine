using Microsoft.FSharp.Core;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Abstractions.Services;
using System;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static Ngine.Backend.Converters.NetworkErrorPrettyPrinter;
using static Ngine.Domain.Schemas.Errors;

namespace Ngine.Infrastructure.Services
{
    public abstract class NetworkIOBase<TNetwork, TLayerConversionError> : INetworkIO<TNetwork>
    {
        private readonly IDeserializer deserializer;
        private readonly ISerializer serializer;

        public INetworkConverter NetworkConverter { get; }

        public abstract IFileFormat FileFormat { get; }

        protected NetworkIOBase(INetworkConverter converter, IDeserializer deserializer, ISerializer serializer)
        {
            NetworkConverter = converter;
            this.deserializer = deserializer;
            this.serializer = serializer;
        }

        public bool TryParse(Schema.Network network, out TNetwork result)
        {
            var parsed = Decode(network);
            if (parsed.IsOk)
            {
                Console.WriteLine("Чтение схемы модели произведено успешно!");
                result = parsed.ResultValue;
                return true;
            }
            else
            {
                var error = Prettify(parsed.ErrorValue);

                Console.WriteLine("Схема модели содержит ошибки - {0}.", error.Length);
                Array.ForEach(error, r => PrintPrettyTree(r));
            }
            result = default;
            return false;
        }

        public bool Read(string fileName, out TNetwork result)
        {
            using var file = File.OpenText(fileName);

            try
            {
                var obj = deserializer.Deserialize<Schema.Network>(file);
                return TryParse(obj, out result);
            }
            catch (YamlException ex)
            {
                Console.WriteLine($"Ошибка при чтении схемы: {ex.Message}");
            }

            result = default;
            return false;
        }

        public void Write(string fileName, TNetwork network)
        {
            var schema = Encode(network);
            var yaml = serializer.Serialize(schema);

            File.WriteAllText(fileName, yaml);
            Console.WriteLine("Схема сохранена в файл {0}", Path.GetFullPath(fileName));
        }

        private static void PrintPrettyTree(PrettyTree pretty, int indents = 0)
        {
            var indent = new string(' ', 3 * indents);

            Console.WriteLine(indent + $"-> {pretty.Item1}:");

            if (!pretty.Item2.Any())
            {
                Console.WriteLine(string.Empty);
            }

            foreach (var dep in pretty.Item2)
            {
                PrintPrettyTree(dep, indents + 1);
            }
        }

        protected abstract FSharpResult<TNetwork, NetworkConversionError<TLayerConversionError>[]> Decode(Schema.Network schema);
        protected abstract Schema.Network Encode(TNetwork network);
        protected abstract PrettyTree[] Prettify(NetworkConversionError<TLayerConversionError>[] error);
    }
}
