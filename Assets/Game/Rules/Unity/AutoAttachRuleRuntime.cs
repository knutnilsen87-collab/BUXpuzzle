using UnityEngine;

namespace Game.Rules.Unity
{
    /// <summary>
    /// Ensures RuleRuntime exists in the scene at runtime.
    /// Looks for GameObject named "GameRoot" first; falls back to creating a "RuleRuntime" GO.
    /// </summary>
    public class AutoAttachRuleRuntime : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuleRuntime()
        {
            if (FindRuleRuntime() != null)
                return;

            GameObject host = GameObject.Find("GameRoot");
            if (host == null)
                host = new GameObject("RuleRuntime");

            host.AddComponent<RuleRuntime>();
            Debug.Log("[AutoAttachRuleRuntime] RuleRuntime attached.");
        }

        private static RuleRuntime FindRuleRuntime()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<RuleRuntime>();
#else
            return Object.FindObjectOfType<RuleRuntime>();
#endif
        }
    }
}