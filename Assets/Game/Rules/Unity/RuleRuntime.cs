using System.Collections.Generic;
using UnityEngine;
using Game.Rules;
using Game.Rules.Rules;
using Game.Config;

namespace Game.Rules.Unity
{
    /// <summary>
    /// Drop this on a GameObject (e.g., GameRoot).
    /// Then call NotifyTurnStart / NotifyAfterResolve from your existing controller when ready.
    /// Safe-by-default: uses debug adapters until you wire real systems.
    /// </summary>
    public class RuleRuntime : MonoBehaviour
    {
        
        // --- FBL HARD-GUARDS: flags + rule definitions ---
        public Game.Config.FeatureFlagsSO FeatureFlags;
        public Game.Rules.RuleDefinitionSO[] RuleDefinitions;
[Header("Feature Flag")]
        public bool Enabled = true;

        [Header("Tuning (v0)")]
        public int DefaultDurationTurns = 5;
        public int Seed = 12345;

        [Header("Debug")]
        public bool DebugLogs = true;

        private RuleManager _manager;
        private RuleContext _ctx;

        // Debug adapters
        private readonly IFeedbackApi _feedback = new DebugFeedbackApi();
        private readonly IRewardsApi _rewards = new DebugRewardsApi();
        private readonly ITelemetryApi _telemetry = new DebugTelemetryApi();
        private readonly IBoardApi _board = new NullBoardApi();

        private void Awake()
        {
            var specs = new List<RuleSpec>
            {
                new RuleSpec { id = RuleId.ComboBonus, durationTurns = DefaultDurationTurns, weight = 100 },
                new RuleSpec { id = RuleId.WildSeed,  durationTurns = DefaultDurationTurns, weight = 100 }
            };

            var pool = new RulePool(specs);

            var rules = new List<IRule>
            {
                new ComboBonusRule(),
                new WildSeedRule()
            };

                        // Apply feature flags (default-safe OFF unless configured)
            Game.Config.FeatureFlags.Apply(FeatureFlags);

            // Build signature registry (blocks silent rules)
            var reg = new RuleSignatureRegistry(RuleDefinitions);

            _manager = new RuleManager(pool, rules, reg) { Enabled = Enabled && Game.Config.FeatureFlags.rules_enabled };

            _ctx = new RuleContext(_board, _feedback, _rewards, _telemetry)
            {
                TurnIndex = 0,
                Seed = Seed,
                LastResolve = new ResolveSummary { merges = 0, combo = 0, hadMatch = false }
            };

            _manager.EnsureAtLeastOneRule(_ctx);

            if (DebugLogs) Debug.Log("[RuleRuntime] Ready. Call NotifyTurnStart / NotifyAfterResolve when wired.");
        }

        private void OnValidate()
        {
            if (_manager != null) _manager.Enabled = Enabled && Game.Config.FeatureFlags.rules_enabled;
        }

        public void NotifyTurnStart(int turnIndex, int seed)
        {
            if (_manager == null) return;

            _ctx.TurnIndex = turnIndex;
            _ctx.Seed = seed;

            _manager.Enabled = Enabled && Game.Config.FeatureFlags.rules_enabled;
            _manager.EnsureAtLeastOneRule(_ctx);
            _manager.OnTurnStart(_ctx);

            if (DebugLogs) Debug.Log($"[RuleRuntime] TurnStart t={turnIndex} seed={seed}");
        }

        public void NotifyAfterResolve(int merges, int combo, bool hadMatch)
        {
            if (_manager == null) return;

            _ctx.LastResolve = new ResolveSummary { merges = merges, combo = combo, hadMatch = hadMatch };

            _manager.Enabled = Enabled && Game.Config.FeatureFlags.rules_enabled;
            _manager.OnAfterResolve(_ctx);
            _manager.EnsureAtLeastOneRule(_ctx);

            if (DebugLogs) Debug.Log($"[RuleRuntime] AfterResolve merges={merges} combo={combo} match={hadMatch}");
        }

        private class DebugFeedbackApi : IFeedbackApi
        {
            public void Emit(string eventName) => Debug.Log($"[Feedback] {eventName}");
        }

        private class DebugRewardsApi : IRewardsApi
        {
            public void AddSoft(int amount, string reason) => Debug.Log($"[Rewards] +{amount} ({reason})");
        }

        private class DebugTelemetryApi : ITelemetryApi
        {
            public void Event(string name, object payload = null)
                => Debug.Log(payload == null ? $"[Telemetry] {name}" : $"[Telemetry] {name} payload={payload}");
        }

        private class NullBoardApi : IBoardApi
        {
            public bool TryMakeRandomTileWild(out int x, out int y)
            {
                x = -1; y = -1;
                return false; // no-op until real board wiring
            }
        }
    }
}