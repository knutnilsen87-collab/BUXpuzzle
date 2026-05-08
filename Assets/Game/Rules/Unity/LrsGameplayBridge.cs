using UnityEngine;

namespace Game.Rules.Unity
{
    /// <summary>
    /// Safe, explicit bridge between gameplay loop and RuleRuntime.
    /// No reflection. No injection. Two calls:
    /// - TurnStart(seed)
    /// - AfterResolve(merges, combo, hadMatch)
    /// </summary>
    public static class LrsGameplayBridge
    {
        private static RuleRuntime _rt;
        private static int _turnIndex = 0;

        private static RuleRuntime RT
        {
            get
            {
                if (_rt == null)
                    _rt = Object.FindFirstObjectByType<RuleRuntime>();
                return _rt;
            }
        }

        public static void TurnStart(int seed)
        {
            if (RT == null) return;
            RT.NotifyTurnStart(_turnIndex, seed);
            Debug.Log("[LRS] TurnStart t=" + _turnIndex + " seed=" + seed);
        }

        public static void AfterResolve(int merges, int combo, bool hadMatch)
        {
            if (RT == null) return;
            RT.NotifyAfterResolve(merges, combo, hadMatch);
            Debug.Log("[LRS] AfterResolve merges=" + merges + " combo=" + combo + " match=" + hadMatch);
            _turnIndex++;
        }
    }
}