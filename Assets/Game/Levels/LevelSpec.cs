namespace Game.Levels
{
    public enum LevelObjectiveType
    {
        MakeMatches = 0,
        ClearTilesOfType = 1,
        ReachScore = 2,
        ClearBlockers = 3,
        DropObjectsToExit = 4,
        CreateSpecials = 5,
        TriggerCascades = 6,
        FinishUnderPar = 7
    }

    public enum LevelMechanic
    {
        None = 0,
        Sunbeam = 1,
        BloomBomb = 2,
        SunOrb = 3,
        Moss = 4,
        Vine = 5,
        DewDrop = 6
    }

    public struct LevelSpec
    {
        public int LevelNumber;
        public int WorldId;
        public string DisplayName;
        public int GoalMatches;
        public int MoveLimit;
        public int ParMoves;
        public int BoardWidth;
        public int BoardHeight;
        public int Seed;
        public string RulesetId;
        public LevelObjectiveType ObjectiveType;
        public int ObjectiveTarget;
        public ObjectiveSpec[] Objectives;
        public int TargetTileType;
        public LevelMechanic NewMechanic;
        public bool IsDailyEligible;
        public bool AllowLose;
        public bool ForceWinBias;
        public string RewardLabel;
        public string DesignerNote;
        public string[] BoardRows;
    }
}
