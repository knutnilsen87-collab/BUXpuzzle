using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(menuName = "Game/Config/Feature Flags", fileName = "FeatureFlags")]
    public class FeatureFlagsSO : ScriptableObject
    {
        public bool rules_enabled = false;
        public bool rules_mutation_enabled = false;
        public bool social_enabled = false;
        public bool rule_text_enabled = false; // QA-only
        [Range(0f, 1f)] public float snapshot_sampling_rate = 0.0f;
    }
}