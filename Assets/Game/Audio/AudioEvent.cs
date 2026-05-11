namespace Game.Audio
{
    public enum AudioEvent
    {
        TileSelect,
        TileDeselect,

        SwapValidStart,
        SwapAcceptedSettle,
        SwapInvalid,

        Match3,
        Match4,
        Match5,

        ClearSmall,
        ClearMedium,
        ClearLarge,

        TileFallShort,
        TileFallLong,
        TileLandSingle,
        TileLandCluster,

        Cascade1,
        Cascade2,
        Cascade3Plus,

        GoalProgress,
        LevelComplete,
        SessionFail,

        UIButtonTap,
        UIOverlayOpen,
        UIOverlayClose,

        TutorialHint,
        HintRepeat,

        SpecialCharge,
        SpecialTrigger,
        SpecialCreated,
        SpecialCombine
    }

    public enum MusicTrack
    {
        RelaxedMenusEasyLevels,
        MainGameplay,
        DeeperFocusLaterLevels,
        PuzzleCalmLoopA,
        PuzzleFocusLoopB,
        CascadeSparkleLayer,
        LevelCompleteStinger
    }

    public enum AmbienceTrack
    {
        NatureLightMorning
    }
}
