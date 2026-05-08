using UnityEngine;

namespace BUXPuzzle.Presentation.Polish
{
    /// <summary>
    /// Phase B scaffold:
    /// Presentation-only driver for clear/match timing.
    /// Must never modify engine truth.
    /// </summary>
    public sealed class FBL_MatchPolishDriver : MonoBehaviour
    {
        [Header("Timing")]
        [Min(0f)] public float clearPulseDuration = 0.08f;
        [Min(0f)] public float clearHoldDuration = 0.04f;
        [Min(0f)] public float matchFeedbackCooldown = 0.02f;

        [Header("Safety")]
        public bool presentationOnly = true;
        public bool enableVerboseLogs = false;

        public void PlayClearFeedback()
        {
            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_MatchPolishDriver] PlayClearFeedback invoked.");
            }
        }
    }
}
