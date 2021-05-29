using System;
using System.Collections.Generic;
using System.Linq;

namespace Ngine.Infrastructure.AppServices
{
    public class LayerIdTracker
    {
        private readonly IDictionary<uint, uint> tracker;

        public LayerIdTracker()
        {
            tracker = new Dictionary<uint, uint>();
        }

        public LayerIdTracker(IEnumerable<(uint, uint)> blackList)
        {
            var maxByLevel = blackList.GroupBy(e => e.Item1).Select(g => KeyValuePair.Create(g.Key, g.Max(e => e.Item2)));
            tracker = new Dictionary<uint, uint>(maxByLevel);
        }

        public Tuple<uint, uint> Generate(uint previousLayer)
        {
            var level = previousLayer + 1;
            tracker[level] = 1u + (tracker.TryGetValue(level, out var curr) ? curr : 0u);

            return Tuple.Create(level, tracker[level]);
        }
    }
}
