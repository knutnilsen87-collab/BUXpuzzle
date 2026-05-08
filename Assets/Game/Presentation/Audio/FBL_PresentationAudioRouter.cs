using UnityEngine;

namespace BUXPuzzle.Presentation.Audio
{
    /// <summary>
    /// Presentation-only audio router scaffold.
    /// Must never mutate engine state.
    /// </summary>
    public sealed class FBL_PresentationAudioRouter : MonoBehaviour
    {
        [SerializeField] private TextAsset audioEventMapJson;
        [SerializeField] private bool enableVerboseLogs = false;

        public void PlayEvent(string eventName)
        {
            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_PresentationAudioRouter] PlayEvent: " + eventName);
            }
        }
    }
}
