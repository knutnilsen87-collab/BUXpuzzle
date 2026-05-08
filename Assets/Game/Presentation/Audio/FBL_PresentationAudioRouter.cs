using UnityEngine;

namespace BUXPuzzle.Presentation.Audio
{
    /// <summary>
    /// Presentation-only audio router scaffold.
    /// Must never mutate engine state.
    /// </summary>
    public sealed class FBL_PresentationAudioRouter : MonoBehaviour
    {
        public static FBL_PresentationAudioRouter Instance { get; private set; }

        [SerializeField] private TextAsset audioEventMapJson;
        [SerializeField] private bool enableVerboseLogs = false;
        [SerializeField] private AudioSource audioSource;

        [Header("Runtime clips")]
        [SerializeField] private AudioClip swapSoft;
        [SerializeField] private AudioClip swapInvalid;
        [SerializeField] private AudioClip matchPop;
        [SerializeField] private AudioClip cascadeRoll;
        [SerializeField] private AudioClip spawnSoft;
        [SerializeField] private AudioClip comboBloom;
        [SerializeField] private AudioClip boardSettle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.65f;
            LoadMissingClips();
        }

        public static FBL_PresentationAudioRouter Ensure()
        {
            if (Instance != null) return Instance;

            var go = new GameObject("FBL_PresentationAudioRouter");
            DontDestroyOnLoad(go);
            return go.AddComponent<FBL_PresentationAudioRouter>();
        }

        public void PlayEvent(string eventName)
        {
            var clip = ClipFor(eventName);
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }

            if (enableVerboseLogs)
            {
                Debug.Log("[FBL_PresentationAudioRouter] PlayEvent: " + eventName);
            }
        }

        private AudioClip ClipFor(string eventName)
        {
            switch (eventName)
            {
                case "swap": return swapSoft;
                case "invalid_swap": return swapInvalid;
                case "match": return matchPop;
                case "cascade": return cascadeRoll;
                case "spawn": return spawnSoft;
                case "combo": return comboBloom;
                case "settle": return boardSettle;
                default: return null;
            }
        }

        private void LoadMissingClips()
        {
            if (swapSoft == null) swapSoft = Resources.Load<AudioClip>("Audio/swap_soft");
            if (swapInvalid == null) swapInvalid = Resources.Load<AudioClip>("Audio/swap_invalid");
            if (matchPop == null) matchPop = Resources.Load<AudioClip>("Audio/match_pop");
            if (cascadeRoll == null) cascadeRoll = Resources.Load<AudioClip>("Audio/cascade_roll");
            if (spawnSoft == null) spawnSoft = Resources.Load<AudioClip>("Audio/spawn_soft");
            if (comboBloom == null) comboBloom = Resources.Load<AudioClip>("Audio/combo_bloom");
            if (boardSettle == null) boardSettle = Resources.Load<AudioClip>("Audio/board_settle");
        }
    }
}
