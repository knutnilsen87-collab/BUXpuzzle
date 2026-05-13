using System.Collections;
using System.Collections.Generic;
using Game.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    public sealed class GameAudioController : MonoBehaviour
    {
        private const string ConfigResourcePath = "Audio/BUXPuzzleAudioConfig";

        private static GameAudioController _instance;

        [SerializeField] private BUXPuzzleAudioConfig config;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup ambienceGroup;
        [SerializeField] private AudioMixerGroup boardSfxGroup;
        [SerializeField] private AudioMixerGroup uiSfxGroup;
        [SerializeField] private AudioMixerGroup rewardSfxGroup;
        [SerializeField] private AudioMixerGroup invalidSfxGroup;

        private readonly Dictionary<AudioEvent, float> _lastPlayedAt = new Dictionary<AudioEvent, float>();
        private readonly Dictionary<AudioEvent, int> _activeVoices = new Dictionary<AudioEvent, int>();
        private readonly HashSet<string> _warnedMissing = new HashSet<string>();

        private AudioSource _boardSfx;
        private AudioSource _uiSfx;
        private AudioSource _rewardSfx;
        private AudioSource _invalidSfx;
        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _ambience;
        private AudioSource _activeMusic;
        private Coroutine _musicFade;
        private Coroutine _ambienceFade;
        private MusicTrack? _currentMusic;
        private AmbienceTrack? _currentAmbience;
        private float _currentMusicBaseVolume;
        private float _currentAmbienceBaseVolume;

        public static GameAudioController Ensure()
        {
            if (_instance != null) return _instance;
            var existing = FindFirstObjectByType<GameAudioController>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var go = new GameObject("GameAudioController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GameAudioController>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
            EnsureSources();
            ApplyVolumes();
        }

        private void Update()
        {
            ApplyVolumes();
        }

        public void Play(AudioEvent audioEvent)
        {
            Play(audioEvent, 1f);
        }

        public void Play(AudioEvent audioEvent, float volumeMultiplier)
        {
            if (!GameSettings.SfxEnabled) return;
            LoadConfig();
            EnsureSources();

            if (config == null || !config.TryGet(audioEvent, out var binding) || binding == null)
            {
                WarnMissing("AudioEvent mapping missing: " + audioEvent);
                return;
            }

            if (binding.Clip == null)
            {
                WarnMissing("AudioEvent clip missing: " + audioEvent);
                return;
            }

            float now = Time.unscaledTime;
            if (_lastPlayedAt.TryGetValue(audioEvent, out float last) && now - last < Mathf.Max(0f, binding.CooldownSeconds))
            {
                return;
            }

            int active = _activeVoices.TryGetValue(audioEvent, out int count) ? count : 0;
            int maxVoices = Mathf.Max(1, binding.MaxSimultaneousVoices);
            if (active >= maxVoices) return;

            var source = SourceFor(binding.Route);
            source.pitch = Random.Range(Mathf.Min(binding.PitchMin, binding.PitchMax), Mathf.Max(binding.PitchMin, binding.PitchMax));
            source.PlayOneShot(binding.Clip, Mathf.Clamp01(volumeMultiplier * binding.Volume * GameSettings.SfxVolume));

            _lastPlayedAt[audioEvent] = now;
            _activeVoices[audioEvent] = active + 1;
            StartCoroutine(ReleaseVoice(audioEvent, binding.Clip.length));
        }

        public void PlayMusic(MusicTrack track, float fadeSeconds = 0.75f)
        {
            LoadConfig();
            EnsureSources();

            if (!GameSettings.MusicEnabled)
            {
                StopMusic(fadeSeconds);
                return;
            }

            if (_currentMusic.HasValue && _currentMusic.Value == track && _activeMusic != null && _activeMusic.isPlaying)
            {
                return;
            }

            if (config == null || !config.TryGet(track, out var binding) || binding == null || binding.Clip == null)
            {
                WarnMissing("MusicTrack clip missing: " + track);
                return;
            }

            if (_musicFade != null) StopCoroutine(_musicFade);

            var from = _activeMusic;
            var to = _activeMusic == _musicA ? _musicB : _musicA;
            to.clip = binding.Clip;
            to.loop = binding.Loop;
            to.volume = 0f;
            to.Play();
            _activeMusic = to;
            _currentMusic = track;
            _currentMusicBaseVolume = binding.Volume;
            _musicFade = StartCoroutine(FadeMusic(from, to, binding.Volume, fadeSeconds));
        }

        public void StopMusic(float fadeSeconds = 0.75f)
        {
            EnsureSources();
            if (_musicFade != null) StopCoroutine(_musicFade);
            _musicFade = StartCoroutine(FadeOutAndStop(_activeMusic, fadeSeconds));
            _currentMusic = null;
        }

        public void PlayAmbience(AmbienceTrack track, float fadeSeconds = 1f)
        {
            LoadConfig();
            EnsureSources();

            if (!GameSettings.AmbienceEnabled)
            {
                StopAmbience(fadeSeconds);
                return;
            }

            if (_currentAmbience.HasValue && _currentAmbience.Value == track && _ambience.isPlaying)
            {
                return;
            }

            if (config == null || !config.TryGet(track, out var binding) || binding == null || binding.Clip == null)
            {
                WarnMissing("AmbienceTrack clip missing: " + track);
                return;
            }

            if (_ambienceFade != null) StopCoroutine(_ambienceFade);
            _ambience.clip = binding.Clip;
            _ambience.loop = binding.Loop;
            _ambience.volume = 0f;
            _ambience.Play();
            _currentAmbience = track;
            _currentAmbienceBaseVolume = binding.Volume;
            _ambienceFade = StartCoroutine(FadeSource(_ambience, binding.Volume * GameSettings.AmbienceVolume, fadeSeconds));
        }

        public void StopAmbience(float fadeSeconds = 1f)
        {
            EnsureSources();
            if (_ambienceFade != null) StopCoroutine(_ambienceFade);
            _ambienceFade = StartCoroutine(FadeOutAndStop(_ambience, fadeSeconds));
            _currentAmbience = null;
        }

        public void SetSfxVolume(float value)
        {
            GameSettings.SfxVolume = value;
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            GameSettings.MusicVolume = value;
            ApplyVolumes();
        }

        public void SetAmbienceVolume(float value)
        {
            GameSettings.AmbienceVolume = value;
            ApplyVolumes();
        }

        private void LoadConfig()
        {
            if (config == null)
            {
                config = Resources.Load<BUXPuzzleAudioConfig>(ConfigResourcePath);
            }
        }

        private void EnsureSources()
        {
            _boardSfx = EnsureSource("BoardSFXSource", boardSfxGroup, _boardSfx);
            _uiSfx = EnsureSource("UISFXSource", uiSfxGroup, _uiSfx);
            _rewardSfx = EnsureSource("RewardSFXSource", rewardSfxGroup, _rewardSfx);
            _invalidSfx = EnsureSource("InvalidSFXSource", invalidSfxGroup, _invalidSfx);
            _musicA = EnsureSource("MusicSourceA", musicGroup, _musicA);
            _musicB = EnsureSource("MusicSourceB", musicGroup, _musicB);
            _ambience = EnsureSource("AmbienceSource", ambienceGroup, _ambience);
            if (_activeMusic == null) _activeMusic = _musicA;
        }

        private AudioSource EnsureSource(string sourceName, AudioMixerGroup group, AudioSource existing)
        {
            if (existing != null) return existing;

            var child = transform.Find(sourceName);
            if (child == null)
            {
                var go = new GameObject(sourceName);
                child = go.transform;
                child.SetParent(transform, false);
            }

            var source = child.GetComponent<AudioSource>();
            if (source == null) source = child.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = group;
            return source;
        }

        private AudioSource SourceFor(AudioRoute route)
        {
            switch (route)
            {
                case AudioRoute.UISFX: return _uiSfx;
                case AudioRoute.RewardSFX: return _rewardSfx;
                case AudioRoute.InvalidSFX: return _invalidSfx;
                default: return _boardSfx;
            }
        }

        private IEnumerator ReleaseVoice(AudioEvent audioEvent, float seconds)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.02f, seconds));
            int active = _activeVoices.TryGetValue(audioEvent, out int count) ? count : 0;
            if (active <= 1) _activeVoices.Remove(audioEvent);
            else _activeVoices[audioEvent] = active - 1;
        }

        private IEnumerator FadeMusic(AudioSource from, AudioSource to, float targetVolume, float seconds)
        {
            float fromStart = from != null ? from.volume : 0f;
            float toTarget = targetVolume * GameSettings.MusicVolume;
            float duration = Mathf.Max(0.01f, seconds);
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
                if (to != null) to.volume = Mathf.Lerp(0f, toTarget, k);
                yield return null;
            }

            if (from != null) from.Stop();
            if (to != null) to.volume = toTarget;
            _musicFade = null;
        }

        private IEnumerator FadeSource(AudioSource source, float targetVolume, float seconds)
        {
            if (source == null) yield break;

            float start = source.volume;
            float duration = Mathf.Max(0.01f, seconds);
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(start, targetVolume, Mathf.Clamp01(t / duration));
                yield return null;
            }

            source.volume = targetVolume;
            if (source == _ambience) _ambienceFade = null;
        }

        private IEnumerator FadeOutAndStop(AudioSource source, float seconds)
        {
            if (source == null) yield break;

            float start = source.volume;
            float duration = Mathf.Max(0.01f, seconds);
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / duration));
                yield return null;
            }

            source.Stop();
            if (source == _activeMusic) _musicFade = null;
            if (source == _ambience) _ambienceFade = null;
        }

        private void ApplyVolumes()
        {
            if (_activeMusic != null && _activeMusic.isPlaying && _musicFade == null)
            {
                _activeMusic.volume = GameSettings.MusicEnabled ? _currentMusicBaseVolume * GameSettings.MusicVolume : 0f;
            }

            if (_ambience != null && _ambience.isPlaying && _ambienceFade == null)
            {
                _ambience.volume = GameSettings.AmbienceEnabled ? _currentAmbienceBaseVolume * GameSettings.AmbienceVolume : 0f;
            }
        }

        private void WarnMissing(string message)
        {
            if (!_warnedMissing.Add(message)) return;
#if UNITY_EDITOR
            Debug.LogWarning("[GameAudioController] " + message);
#endif
        }
    }
}
