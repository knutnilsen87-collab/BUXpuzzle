using System;
using UnityEngine;

namespace Game.Audio
{
    [CreateAssetMenu(menuName = "BUXPuzzle/Audio/BUXPuzzle Audio Config")]
    public sealed class BUXPuzzleAudioConfig : ScriptableObject
    {
        public AudioEventBinding[] Events;
        public MusicTrackBinding[] Music;
        public AmbienceTrackBinding[] Ambience;

        public bool TryGet(AudioEvent audioEvent, out AudioEventBinding binding)
        {
            if (Events != null)
            {
                for (int i = 0; i < Events.Length; i++)
                {
                    if (Events[i] != null && Events[i].Event == audioEvent)
                    {
                        binding = Events[i];
                        return true;
                    }
                }
            }

            binding = null;
            return false;
        }

        public bool TryGet(MusicTrack track, out MusicTrackBinding binding)
        {
            if (Music != null)
            {
                for (int i = 0; i < Music.Length; i++)
                {
                    if (Music[i] != null && Music[i].Track == track)
                    {
                        binding = Music[i];
                        return true;
                    }
                }
            }

            binding = null;
            return false;
        }

        public bool TryGet(AmbienceTrack track, out AmbienceTrackBinding binding)
        {
            if (Ambience != null)
            {
                for (int i = 0; i < Ambience.Length; i++)
                {
                    if (Ambience[i] != null && Ambience[i].Track == track)
                    {
                        binding = Ambience[i];
                        return true;
                    }
                }
            }

            binding = null;
            return false;
        }
    }

    public enum AudioRoute
    {
        BoardSFX,
        UISFX,
        RewardSFX,
        InvalidSFX
    }

    [Serializable]
    public sealed class AudioEventBinding
    {
        public AudioEvent Event;
        public AudioRoute Route = AudioRoute.BoardSFX;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.75f;
        public float PitchMin = 0.97f;
        public float PitchMax = 1.03f;
        public float CooldownSeconds = 0.04f;
        public int MaxSimultaneousVoices = 4;
    }

    [Serializable]
    public sealed class MusicTrackBinding
    {
        public MusicTrack Track;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.45f;
        public bool Loop = true;
    }

    [Serializable]
    public sealed class AmbienceTrackBinding
    {
        public AmbienceTrack Track;
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.22f;
        public bool Loop = true;
    }
}
