using UnityEngine;
using UnityEngine.UI;

namespace BUXPuzzle.Presentation.HUD
{
    /// <summary>
    /// HUD scaffold for Phase B.
    /// Presentation-only. Reads state from external sources; does not own gameplay truth.
    /// </summary>
    public sealed class FBL_HUDPresenter : MonoBehaviour
    {
        [Header("Bindings")]
        [SerializeField] private Text movesText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Text comboText;

        [Header("Safety")]
        [SerializeField] private bool enableVerboseLogs = false;

        public void SetMoves(int moves)
        {
            if (movesText != null) movesText.text = moves.ToString();
        }

        public void SetScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString();
        }

        public void SetFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
            if (enableVerboseLogs) Debug.Log("[FBL_HUDPresenter] Feedback: " + message);
        }

        public void SetCombo(string message)
        {
            if (comboText != null) comboText.text = message;
        }
    }
}
