using UnityEngine;
using BUXPuzzle.Presentation.HUD;

namespace BUXPuzzle.Presentation.SceneBindings
{
    /// <summary>
    /// Scene-safe binder scaffold for HUD references.
    /// This class does not own gameplay truth; it binds existing presentation references.
    /// </summary>
    public sealed class FBL_HUDSceneBinder : MonoBehaviour
    {
        [SerializeField] private FBL_HUDPresenter hudPresenter;
        [SerializeField] private MonoBehaviour sourceMovesProvider;
        [SerializeField] private MonoBehaviour sourceScoreProvider;

        [ContextMenu("FBL Validate HUD Binding")]
        public void ValidateBinding()
        {
            Debug.Log("[FBL_HUDSceneBinder] ValidateBinding hudPresenter=" + (hudPresenter != null));
        }
    }
}
