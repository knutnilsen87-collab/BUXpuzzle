using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// 🔒 LÅST FEATURE FLAG POLICY (min-sett)
    /// - rules_enabled
    /// - rules_mutation_enabled
    /// - social_enabled
    /// - rule_text_enabled (QA-only)
    /// - snapshot_sampling_rate
    ///
    /// Default-safe: new flags default OFF unless explicitly enabled via config.
    /// </summary>
    public static class FeatureFlags
    {
        // Default-safe OFF
        public static bool rules_enabled = false;
        public static bool rules_mutation_enabled = false;
        public static bool social_enabled = false;
        public static bool rule_text_enabled = false; // QA-only
        public static float snapshot_sampling_rate = 0.0f; // 0..1

        public static void Apply(FeatureFlagsSO so)
        {
            if (so == null) return;
            rules_enabled = so.rules_enabled;
            rules_mutation_enabled = so.rules_mutation_enabled;
            social_enabled = so.social_enabled;
            rule_text_enabled = so.rule_text_enabled;
            snapshot_sampling_rate = Mathf.Clamp01(so.snapshot_sampling_rate);
        }
    }
}