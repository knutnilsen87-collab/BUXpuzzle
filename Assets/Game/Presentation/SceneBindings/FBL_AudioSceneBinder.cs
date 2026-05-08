using UnityEngine;
using BUXPuzzle.Presentation.Audio;

namespace BUXPuzzle.Presentation.SceneBindings
{
    /// <summary>
    /// Scene-safe binder scaffold for presentation audio routing.
    /// Presentation-only; must not mutate runtime truth.
    /// </summary>
    public sealed class FBL_AudioSceneBinder : MonoBehaviour
    {
        [SerializeField] private FBL_PresentationAudioRouter audioRouter;
        [SerializeField] private AudioSource primaryAudioSource;

        [ContextMenu("FBL Validate Audio Binding")]
        public void ValidateBinding()
        {
            Debug.Log("[FBL_AudioSceneBinder] ValidateBinding audioRouter=" + (audioRouter != null) + " primaryAudioSource=" + (primaryAudioSource != null));
        }
    }
}
