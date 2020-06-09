using System;
using System.Collections.Generic;

namespace Ngine.Infrastructure.AppServices
{
    public class LayerIdTracker
    {
        private readonly IDictionary<uint, uint> tracker = new Dictionary<uint, uint>();

        public Tuple<uint, uint> Generate(uint previousLayer)
        {
            var level = previousLayer + 1;
            tracker[level] = 1u + (tracker.TryGetValue(level, out var curr) ? curr : 0u);

            return Tuple.Create(level, tracker[level]);
        }
    }
}
