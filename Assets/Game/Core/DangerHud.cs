using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Always-visible danger indicator (no text dependency required).
    /// Uses OnGUI so it's guaranteed visible even without Canvas setup.
    /// If you later add a Canvas meter, keep this as debug/QA overlay.
    /// </summary>
    public sealed class DangerHud : MonoBehaviour
    {
        [SerializeField] private Vector2 pos = new Vector2(16, 16);
        [SerializeField] private Vector2 size = new Vector2(260, 18);

        void OnGUI()
        {
            var ds = DangerSystem.I;
            if (ds == null) return;

            float t = Mathf.Clamp01(ds.danger / ds.FailAt);
            var rBg = new Rect(pos.x, pos.y, size.x, size.y);
            var rFg = new Rect(pos.x, pos.y, size.x * t, size.y);

            // Background
            GUI.Box(rBg, GUIContent.none);

            // Foreground bar
            Color prev = GUI.color;
            // We avoid "explanatory text". This is pure state readability.
            GUI.color = Color.Lerp(new Color(0.2f, 0.8f, 0.2f, 1f), new Color(0.9f, 0.2f, 0.2f, 1f), t);
            GUI.Box(rFg, GUIContent.none);
            GUI.color = prev;
        }
    }
}