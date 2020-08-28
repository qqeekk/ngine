using Ngine.Domain.Execution;
using Ngine.Domain.Schemas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace Ngine.Infrastructure.Services
{
    public class KerasNetworkIO
    {
        private readonly ISerializer serializer;
        private readonly INetworkGenerator generator;

        public KerasNetworkIO(ISerializer serializer,
                              INetworkGenerator generator)
        {
            this.serializer = serializer;
            this.generator = generator;
        }

        private void WriteAmbiguities(string modelFileName, Schema.AmbiguityMapProduct ambiguities)
        {
            Console.WriteLine("Model saved to file {0}", modelFileName);

            if (ambiguities.Ambiguities.Length > 0)
            {
                var ambiguitiesYaml = serializer.Serialize(ambiguities);
                var ambiguitiesPath = Path.ChangeExtension(modelFileName, "ambiguities.yaml");
                File.WriteAllText(ambiguitiesPath, ambiguitiesYaml);

                Console.WriteLine("Ambiguities ({0}) saved to file {1}", ambiguities.Ambiguities.Length, ambiguitiesPath);
            }
        }

        public void Write(string folderName, Network network)
        {
            var (model, ambiguities) = generator.SaveModel(folderName, network);
            WriteAmbiguities(model, ambiguities);
        }

        public void Write(Network network)
        {
            var (model, ambiguities) = generator.SaveModel(network);
            WriteAmbiguities(model, ambiguities);
        }
    }
}
