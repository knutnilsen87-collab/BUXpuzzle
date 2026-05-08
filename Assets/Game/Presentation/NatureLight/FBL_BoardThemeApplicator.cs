using UnityEngine;

namespace Game.Presentation.NatureLight
{
    public sealed class FBL_BoardThemeApplicator : MonoBehaviour
    {
        [SerializeField] private FBL_NatureLightTheme theme;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Renderer boardPanelRenderer;
        [SerializeField] private SpriteRenderer boardPanelSprite;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            if (theme == null) return;

            if (targetCamera != null)
            {
                targetCamera.backgroundColor = theme.midHaze;
            }

            if (boardPanelRenderer != null)
            {
                boardPanelRenderer.material.color = theme.boardTint;
            }

            if (boardPanelSprite != null)
            {
                boardPanelSprite.color = theme.boardTint;
            }
        }
    }
}
