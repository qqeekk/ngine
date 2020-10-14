using Ngine.Domain.Schemas;
using Ngine.Infrastructure.Abstractions.Services;
using System;
using System.Collections.Generic;
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
            var records = deserializer.Deserialize<AmbiguityMapProduct>(ambiguitiesFile);
            return records.Ambiguities.ToDictionary(a => AmbiguityVariableName.NewVariable(a.Name), a => uint.Parse(a.Value));
        }
    }
}
