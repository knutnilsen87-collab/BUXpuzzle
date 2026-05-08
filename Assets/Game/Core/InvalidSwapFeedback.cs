using UnityEngine;

namespace Game.Core
{
    public class InvalidSwapFeedback : MonoBehaviour
    {
        public static void Show(Vector3 a, Vector3 b)
        {
            Debug.Log("No match");
        }
    }
}
