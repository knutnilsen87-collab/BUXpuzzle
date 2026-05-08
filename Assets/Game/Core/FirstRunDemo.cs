using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// 1-step onboarding overlay. No deep rules, just the input model.
    /// </summary>
    public sealed class FirstRunDemo : MonoBehaviour
    {
        private static bool _dismissed = false;

        void OnGUI()
        {
            if (_dismissed) return;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

            var r = new Rect(Screen.width/2 - 180, Screen.height/2 - 60, 360, 120);
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 12, r.y + 14, r.width - 24, 40),
                "Swap two adjacent tiles to make 3 in a row.");

            if (GUI.Button(new Rect(r.x + r.width/2 - 60, r.y + 64, 120, 32), "OK"))
                _dismissed = true;
        }
    }
}
