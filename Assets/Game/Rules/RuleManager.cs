using System.Collections.Generic;

namespace Game.Rules
{
    public class RuleManager
    {
        
        // --- FBL HARD-GUARDS: signature registry + fallback ladder ---
        private readonly IRuleSignatureRegistry _sig;
public const int MaxActiveRules = 3;

        public bool Enabled { get; set; } = true;

        private readonly Dictionary<RuleId, IRule> _rules = new();
        private readonly List<ActiveRule> _active = new();
        private readonly RulePool _pool;

        public IReadOnlyList<ActiveRule> Active => _active;

        public RuleManager(RulePool pool, IEnumerable<IRule> rules, IRuleSignatureRegistry sig = null) {
            _sig = sig;
            _pool = pool;
            foreach (var r in rules) _rules[r.Id] = r;
        }

        public void EnsureAtLeastOneRule(RuleContext ctx)
        {
            if (!Enabled) return;
            if (_active.Count > 0) return;

            var exclude = new HashSet<RuleId>();
            var spec = _pool.PickNext(exclude);
            if (spec == null) return;

            _active.Add(new ActiveRule(spec.id, spec.durationTurns));
            ctx.Telemetry?.Event("rule_added", new { id = spec.id.ToString(), turns = spec.durationTurns, seed = ctx.Seed });
            ctx.Feedback?.Emit("rule_change");
        }

        public void OnTurnStart(RuleContext ctx)
        {
            if (!Enabled) return;

            for (int i = 0; i < _active.Count; i++)
            {
                var id = _active[i].id;
                var rule = _rules[id];
                if (rule.CanTrigger(ctx)) rule.OnTurnStart(ctx);
            }
        }

        public void OnAfterResolve(RuleContext ctx)
        {
            if (!Enabled) return;

            for (int i = 0; i < _active.Count; i++)
            {
                var id = _active[i].id;
                var rule = _rules[id];
                if (rule.CanTrigger(ctx)) rule.OnAfterResolve(ctx);
            }

            TickAndExpire(ctx);
        }

        private void TickAndExpire(RuleContext ctx)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                _active[i].turnsLeft--;

                if (_active[i].turnsLeft <= 0)
                {
                    var id = _active[i].id;
                    _rules[id].OnExpire(ctx);
                    _active.RemoveAt(i);

                    ctx.Telemetry?.Event("rule_expired", new { id = id.ToString() });
                    ctx.Feedback?.Emit("rule_change");
                }
            }
        }
    }
}