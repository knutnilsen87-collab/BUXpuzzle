using UnityEngine;
using BUXPuzzle.Presentation.SceneBindings;


using BUXPuzzle.Presentation.HUD;

using BUXPuzzle.Presentation.Audio;
namespace BUXPuzzle.Presentation.Bootstrap
{
    /// <summary>
    /// Phase B presentation bootstrap scaffold.
    /// Scene-safe helper for locating/binding HUD and audio binders.
    /// Does not own gameplay truth.
    /// </summary>
    public sealed class FBL_PresentationBootstrapPhaseB : MonoBehaviour
    {
        public void FBL_ValidatePlayableBuildBindings()
        {
            Debug.Log("[FBL_PresentationBootstrapPhaseB] hudPresenter=" + (hudPresenter != null) + " audioRouter=" + (audioRouter != null));
        }

        [SerializeField] private FBL_PresentationAudioRouter audioRouter;

        [SerializeField] private FBL_HUDPresenter hudPresenter;

        [SerializeField] private FBL_HUDSceneBinder hudSceneBinder;
        [SerializeField] private FBL_AudioSceneBinder audioSceneBinder;
        [SerializeField] private bool enableVerboseLogs = false;

        [ContextMenu("FBL Validate Phase B Presentation Bootstrap")]
        public void ValidateBindings()
        {
            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_PresentationBootstrapPhaseB] hudSceneBinder=" + (hudSceneBinder != null) + " audioSceneBinder=" + (audioSceneBinder != null));
            }
        }
    }
}

