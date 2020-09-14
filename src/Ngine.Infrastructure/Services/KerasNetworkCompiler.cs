using Microsoft.FSharp.Core;
using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Abstractions.Services;
using Ngine.Infrastructure.Services.FileFormats;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Ngine.Infrastructure.Services
{
    public class KerasNetworkCompiler : INetworkCompiler
    {
        private readonly ISerializer serializer;

        public KerasNetworkCompiler(ISerializer serializer,
                                    INetworkGenerator generator)
        {
            this.serializer = serializer;
            NetworkGenerator = generator;
        }

        public INetworkGenerator NetworkGenerator { get; }

        private FSharpOption<string> WriteAmbiguities(string modelFileName, Schema.AmbiguityMapProduct ambiguities)
        {
            Console.WriteLine("Model saved to file {0}", modelFileName);

            if (ambiguities.Ambiguities.Length > 0)
            {
                var ambiguitiesYaml = serializer.Serialize(ambiguities);
                var ambiguitiesPath = Path.ChangeExtension(modelFileName, $"ambiguities.{new NgineSchemaAmbiguitiesFormat().FileExtension}");
                File.WriteAllText(ambiguitiesPath, ambiguitiesYaml);

                Console.WriteLine("Ambiguities ({0}) saved to file {1}", ambiguities.Ambiguities.Length, ambiguitiesPath);
                return ambiguitiesPath;
            }

            return FSharpOption<string>.None;
        }

        public INetworkCompilerOutput Write(string folderName, Network network)
        {
            var (model, ambiguities) = NetworkGenerator.SaveModel(folderName, network);
            var ambiguitiesPath = WriteAmbiguities(model, ambiguities);

            return new KerasNetworkCompilerOutput
            {
                CompiledNetworkPath = model,
                AmbiguitiesPath = ambiguitiesPath,
            };
        }
    }
}
