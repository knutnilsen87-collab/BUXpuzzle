using UnityEngine;

namespace Game.Presentation.Juice
{
    public sealed class ScreenPulse : MonoBehaviour
    {
        private float _until;
        private Texture2D _texture;

        public void Pulse(float seconds = 0.32f)
        {
            _until = Time.unscaledTime + Mathf.Max(0.08f, seconds);
        }

        private void OnGUI()
        {
            if (Time.unscaledTime > _until) return;
            if (_texture == null)
            {
                _texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _texture.SetPixel(0, 0, new Color(1f, 0.96f, 0.65f, 1f));
                _texture.Apply();
            }

            float alpha = Mathf.Clamp01((_until - Time.unscaledTime) / 0.32f) * 0.08f;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _texture);
            GUI.color = Color.white;
        }
    }
}
