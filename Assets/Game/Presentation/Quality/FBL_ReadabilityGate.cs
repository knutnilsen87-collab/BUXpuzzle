using UnityEngine;

namespace BUXPuzzle.Presentation.Quality
{
    /// <summary>
    /// Readability gate scaffold.
    /// This class collects future signals but does not auto-claim subjective visual quality pass.
    /// </summary>
    public sealed class FBL_ReadabilityGate : MonoBehaviour
    {
        [SerializeField] private bool enableVerboseLogs = false;

        public void RecordSignal(string signalName, string value)
        {
            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_ReadabilityGate] " + signalName + "=" + value);
            }
        }
    }
}
