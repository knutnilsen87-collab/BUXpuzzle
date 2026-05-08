using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    /// <summary>
    /// MVP round flow: when danger collapses -> reload active scene.
    /// Later you can add result screen, checkpoints, meta progression.
    /// </summary>
    public sealed class RoundFlow : MonoBehaviour
    {
        void Awake()
        {
            if (DangerSystem.I != null)
                DangerSystem.I.OnRoundOver += HandleRoundOver;
        }

        void OnDestroy()
        {
            if (DangerSystem.I != null)
                DangerSystem.I.OnRoundOver -= HandleRoundOver;
        }

        private void HandleRoundOver()
        {
            // Minimal, deterministic reset:
            var s = SceneManager.GetActiveScene();
            SceneManager.LoadScene(s.buildIndex);
        }
    }
}