using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI
{
    public sealed class RewardCard : MonoBehaviour
    {
        public Text Title;
        public Text Value;

        public void Show(string title, string value)
        {
            if (Title != null) Title.text = title ?? string.Empty;
            if (Value != null) Value.text = value ?? string.Empty;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
