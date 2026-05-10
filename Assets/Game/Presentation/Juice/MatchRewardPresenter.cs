using UnityEngine;

namespace Game.Presentation.Juice
{
    public sealed class MatchRewardPresenter : MonoBehaviour
    {
        private string _message;
        private float _until;
        private GUIStyle _style;

        public void ShowCascade(int iteration)
        {
            if (iteration <= 1) return;
            _message = iteration == 2 ? "Fin flyt" : (iteration == 3 ? "Kjede!" : "Nydelig");
            _until = Time.unscaledTime + 0.85f;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_message) || Time.unscaledTime > _until) return;
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.94f, 0.62f, 0.94f) }
                };
            }

            float t = Mathf.Clamp01((_until - Time.unscaledTime) / 0.85f);
            float y = Mathf.Lerp(Screen.height * 0.38f - 18f, Screen.height * 0.38f, t);
            GUI.Label(new Rect(0f, y, Screen.width, 34f), _message, _style);
        }
    }
}
