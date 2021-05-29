using Ngine.Domain.Schemas;
using System.Collections.Generic;

namespace Ngine.Infrastructure.Abstractions.Services
{
    public interface IAmbiguitiesIO
    {
        IDictionary<AmbiguityVariableName, uint> ReadResolvedAmbiguities(string ambiguitiesFile);
    }
}
