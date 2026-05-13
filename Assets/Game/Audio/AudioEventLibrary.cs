using UnityEngine;

namespace Game.Audio
{
    public enum AudioEventId
    {
        ButtonTap,
        TileSelect,
        TileSwap,
        InvalidSwap,
        TileMatch,
        TileDrop,
        Cascade1,
        Cascade2,
        ResultOpen,
        RewardPop,
        NextLevel,
        LeafMatch,
        DewMatch,
        SunseedMatch,
        BerryMatch,
        MoonflowerMatch,
        PebbleMatch,
        SunbeamActivate,
        BloomBombActivate,
        ColorClearActivate,
        GardenUnlock,
        DailyComplete,
        ShareTap
    }

    [CreateAssetMenu(menuName = "BUXPuzzle/Audio/Audio Event Library")]
    public sealed class AudioEventLibrary : ScriptableObject
    {
        public AudioEventEntry[] Events;
    }

    [System.Serializable]
    public sealed class AudioEventEntry
    {
        public AudioEventId Id;
        public AudioClip[] Clips;
        [Range(0f, 1f)] public float Volume = 1f;
        public float PitchMin = 0.96f;
        public float PitchMax = 1.04f;
        public float CooldownSeconds = 0.02f;
    }
}
