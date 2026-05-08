using System.Collections.Generic;
using System.Linq;

namespace Game.Rules
{
    public class RulePool
    {
        private readonly List<RuleSpec> _specs;

        public RulePool(IEnumerable<RuleSpec> specs)
        {
            _specs = specs.ToList();
        }

        // Minimal deterministic pick (v0): first non-excluded.
        public RuleSpec PickNext(HashSet<RuleId> exclude)
        {
            foreach (var s in _specs)
                if (!exclude.Contains(s.id))
                    return s;
            return null;
        }
    }
}