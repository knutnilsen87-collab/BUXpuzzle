using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Always-visible top HUD: Objective + Moves + Progress.
    /// MVP: counts 'matches' via public method OnMatch(); we can wire later.
    /// </summary>
    public sealed class SimpleHud : MonoBehaviour
    {
        public int targetMatches = 10;
        public int movesLeft = 20;

        private int _matches = 0;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10,10,320,110), GUI.skin.box);
            GUILayout.Label($"Objective: Make {targetMatches} matches");
            GUILayout.Label($"Moves left: {movesLeft}");
            float p = Mathf.Clamp01(targetMatches <= 0 ? 0f : (float)_matches / targetMatches);
            GUILayout.HorizontalSlider(p, 0f, 1f);
            GUILayout.EndArea();
        }

        public void OnMatch()
        {
            _matches++;
            movesLeft = Mathf.Max(0, movesLeft - 1);
        }
    }
}
