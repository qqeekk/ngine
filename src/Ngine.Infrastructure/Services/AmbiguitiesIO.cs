using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using static Ngine.Domain.Schemas.Schema;

namespace Ngine.Infrastructure.Services
{
    public class AmbiguitiesIO : IAmbiguitiesIO
    {
        private readonly IDeserializer deserializer;

        public AmbiguitiesIO(IDeserializer deserializer)
        {
            this.deserializer = deserializer;
        }
        
        public IDictionary<AmbiguityVariableName, uint> ReadResolvedAmbiguities(string ambiguitiesFile)
        {
            var content = File.ReadAllText(ambiguitiesFile);
            var records = deserializer.Deserialize<AmbiguityMapProduct>(content);
            return records.Ambiguities.ToDictionary(a => AmbiguityVariableName.NewVariable(a.Name), a => uint.Parse(a.Value));
        }
    }
}
