using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Global pressure system: Danger rises each resolved turn. When it reaches threshold -> round ends.
    /// This is the core of the game: "each move makes things safer or riskier".
    /// </summary>
    public sealed class DangerSystem : MonoBehaviour
    {
        public static DangerSystem I { get; private set; }

        [Header("Runtime")]
        [Range(0f, 100f)] public float danger = 0f;

        [Header("Tuning")]
        [SerializeField] private float perTurnIncrease = 6;
        [SerializeField] private float failAt = 100;

        public float PerTurnIncrease => perTurnIncrease;
        public float FailAt => failAt;
        public bool IsDead => danger >= failAt;

        public System.Action<float> OnDangerChanged;     // value 0..failAt
        public System.Action OnRoundOver;                // collapse reached

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
            Publish();
        }

        public void ResetRun(float start = 0f)
        {
            danger = Mathf.Clamp(start, 0f, failAt);
            Publish();
        }

        public void Add(float amount)
        {
            danger = Mathf.Clamp(danger + amount, 0f, failAt);
            Publish();
            if (IsDead) RoundOver();
        }

        public void Reduce(float amount)
        {
            danger = Mathf.Clamp(danger - Mathf.Abs(amount), 0f, failAt);
            Publish();
        }

        public void OnTurnResolved()
        {
            Add(perTurnIncrease);
        }

        private void Publish()
        {
            OnDangerChanged?.Invoke(danger);
        }

        private void RoundOver()
        {
            Debug.Log("[DANGER] System collapsed — round over");
            OnRoundOver?.Invoke();
        }
    }
}