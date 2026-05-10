using System.Collections.Generic;
using Game.Settings;
using UnityEngine;

namespace Game.Audio
{
    public sealed class GameAudioController : MonoBehaviour
    {
        private static GameAudioController _instance;
        private readonly Dictionary<AudioEvent, AudioClip> _clips = new Dictionary<AudioEvent, AudioClip>();
        private AudioSource _source;
        private float _lastDropLandAt = -10f;

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
            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.volume = 0.78f;
            BuildClips();
        }

        public void Play(AudioEvent evt, float volume = 1f)
        {
            if (!GameSettings.SfxEnabled || _source == null) return;

            if (evt == AudioEvent.DropLand)
            {
                if (Time.unscaledTime - _lastDropLandAt < 0.10f) return;
                _lastDropLandAt = Time.unscaledTime;
            }

            if (!_clips.TryGetValue(evt, out var clip) || clip == null) return;

            float jitter = Random.Range(0.96f, 1.04f);
            _source.pitch = PitchFor(evt) * jitter;
            _source.PlayOneShot(clip, Mathf.Clamp01(volume * VolumeFor(evt)));
        }

        private void BuildClips()
        {
            _clips[AudioEvent.TileSelect] = Tone("tile_select", 620f, 0.045f, 0.10f);
            _clips[AudioEvent.TileDeselect] = Tone("tile_deselect", 420f, 0.035f, 0.08f);
            _clips[AudioEvent.SwapAccept] = Tone("swap_accept", 520f, 0.070f, 0.12f);
            _clips[AudioEvent.SwapReject] = Tone("swap_reject", 220f, 0.090f, 0.11f);
            _clips[AudioEvent.Match3] = Chime("match_3", 520f, 660f, 0.135f, 0.16f);
            _clips[AudioEvent.Match4] = Chime("match_4", 560f, 740f, 0.150f, 0.18f);
            _clips[AudioEvent.Match5] = Chime("match_5", 620f, 840f, 0.175f, 0.20f);
            _clips[AudioEvent.Clear] = Noise("clear", 0.16f, 0.07f);
            _clips[AudioEvent.DropLand] = Tone("drop_land", 170f, 0.055f, 0.08f);
            _clips[AudioEvent.Cascade1] = Chime("cascade_1", 520f, 700f, 0.13f, 0.14f);
            _clips[AudioEvent.Cascade2] = Chime("cascade_2", 620f, 830f, 0.15f, 0.16f);
            _clips[AudioEvent.Cascade3Plus] = Chime("cascade_3_plus", 740f, 960f, 0.18f, 0.18f);
            _clips[AudioEvent.GoalProgress] = Tone("goal_progress", 760f, 0.075f, 0.10f);
            _clips[AudioEvent.LevelComplete] = Chime("level_complete", 520f, 880f, 0.45f, 0.24f);
            _clips[AudioEvent.ButtonTap] = Tone("button_tap", 540f, 0.045f, 0.08f);
            _clips[AudioEvent.TutorialHint] = Chime("tutorial_hint", 480f, 640f, 0.20f, 0.12f);
        }

        private static float VolumeFor(AudioEvent evt)
        {
            switch (evt)
            {
                case AudioEvent.SwapReject: return 0.42f;
                case AudioEvent.DropLand: return 0.38f;
                case AudioEvent.Clear: return 0.34f;
                case AudioEvent.LevelComplete: return 0.65f;
                default: return 0.48f;
            }
        }

        private static float PitchFor(AudioEvent evt)
        {
            switch (evt)
            {
                case AudioEvent.Cascade2: return 1.04f;
                case AudioEvent.Cascade3Plus: return 1.08f;
                case AudioEvent.SwapReject: return 0.92f;
                default: return 1f;
            }
        }

        private static AudioClip Tone(string name, float hz, float seconds, float attack)
        {
            return Build(name, seconds, i =>
            {
                float t = i / 44100f;
                float env = Envelope(t, seconds, attack);
                return Mathf.Sin(t * hz * Mathf.PI * 2f) * env;
            });
        }

        private static AudioClip Chime(string name, float lowHz, float highHz, float seconds, float attack)
        {
            return Build(name, seconds, i =>
            {
                float t = i / 44100f;
                float env = Envelope(t, seconds, attack);
                float a = Mathf.Sin(t * lowHz * Mathf.PI * 2f);
                float b = Mathf.Sin(t * highHz * Mathf.PI * 2f) * 0.65f;
                return (a + b) * 0.5f * env;
            });
        }

        private static AudioClip Noise(string name, float seconds, float attack)
        {
            return Build(name, seconds, i =>
            {
                float t = i / 44100f;
                float env = Envelope(t, seconds, attack);
                return Random.Range(-1f, 1f) * env * 0.38f;
            });
        }

        private static AudioClip Build(string name, float seconds, System.Func<int, float> sample)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(44100f * seconds));
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = Mathf.Clamp(sample(i), -0.65f, 0.65f);
            }

            var clip = AudioClip.Create(name, count, 1, 44100, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Envelope(float t, float seconds, float attack)
        {
            float a = Mathf.Clamp01(t / Mathf.Max(0.001f, attack));
            float r = Mathf.Clamp01((seconds - t) / Mathf.Max(0.001f, seconds * 0.72f));
            return Mathf.SmoothStep(0f, 1f, a) * Mathf.SmoothStep(0f, 1f, r);
        }
    }
}
