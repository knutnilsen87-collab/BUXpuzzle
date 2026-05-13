using System;

namespace Game.Levels
{
    [Serializable]
    public sealed class LevelSpecV2
    {
        public int LevelId;
        public int WorldId;
        public string DisplayName;
        public int Width;
        public int Height;
        public int MoveLimit;
        public int ParMoves;
        public int Seed;
        public bool AllowLose;
        public bool ForceWinBias;
        public string RulesetId = "main_path_v1";
        public string DesignerNote;
        public LevelMechanic NewMechanic;
        public ObjectiveSpec[] Objectives = Array.Empty<ObjectiveSpec>();
        public TutorialSpec Tutorial = new TutorialSpec();
        public DifficultySpec Difficulty = new DifficultySpec();
        public string[] BoardRows = Array.Empty<string>();
    }

    [Serializable]
    public sealed class ObjectiveSpec
    {
        public LevelObjectiveType Type;
        public int Target;
        public int TargetTileType = -1;
    }

    [Serializable]
    public sealed class TutorialSpec
    {
        public string Key;
        public bool StrongHint;
    }

    [Serializable]
    public sealed class DifficultySpec
    {
        public int Stars1;
        public int Stars2;
        public int Stars3;
    }
}
