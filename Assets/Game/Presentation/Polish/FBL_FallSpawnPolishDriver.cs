using UnityEngine;

namespace BUXPuzzle.Presentation.Polish
{
    /// <summary>
    /// Phase B scaffold:
    /// Presentation-only driver for gravity/spawn/settle timing.
    /// Must never modify engine truth.
    /// </summary>
    public sealed class FBL_FallSpawnPolishDriver : MonoBehaviour
    {
        [Header("Gravity")]
        [Min(0f)] public float fallDurationPerCell = 0.045f;
        [Min(0f)] public float landingPunchDuration = 0.05f;

        [Header("Spawn")]
        [Min(0f)] public float spawnFadeDuration = 0.06f;
        [Min(0f)] public float settleDelay = 0.03f;

        [Header("Safety")]
        public bool presentationOnly = true;
        public bool enableVerboseLogs = false;

        public void PlayFallFeedback()
        {
            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_FallSpawnPolishDriver] PlayFallFeedback invoked.");
            }
        }

        public void PlaySpawnFeedback()
        {
            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_FallSpawnPolishDriver] PlaySpawnFeedback invoked.");
            }
        }
    }
}
