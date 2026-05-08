using UnityEngine;

namespace Game.Presentation.NatureLight
{
    public sealed class FBL_ThemeBootstrap : MonoBehaviour
    {
        [SerializeField] private FBL_NatureLightTheme theme;
        [SerializeField] private FBL_BoardThemeApplicator boardApplicator;
        [SerializeField] private FBL_TileVisualBinder[] tileBinders;

        private void Awake()
        {
            if (boardApplicator == null)
            {
                boardApplicator = FindObjectOfType<FBL_BoardThemeApplicator>();
            }

            if (tileBinders == null || tileBinders.Length == 0)
            {
                tileBinders = FindObjectsOfType<FBL_TileVisualBinder>(true);
            }
        }

        private void Start()
        {
            if (theme == null) return;

            if (boardApplicator != null)
            {
                var field = typeof(FBL_BoardThemeApplicator).GetField("theme", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(boardApplicator, theme);
                }
                boardApplicator.Apply();
            }

            if (tileBinders != null)
            {
                foreach (var binder in tileBinders)
                {
                    if (binder == null) continue;

                    var field = typeof(FBL_TileVisualBinder).GetField("theme", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(binder, theme);
                    }
                    binder.ApplyTheme();
                }
            }
        }
    }
}
