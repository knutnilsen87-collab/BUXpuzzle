using System;

namespace Game.Levels
{
    [Serializable]
    internal sealed class LevelJsonDto
    {
        public int levelId;
        public int worldId;
        public string displayName;
        public int width;
        public int height;
        public int moveLimit;
        public int parMoves;
        public int seed;
        public string rulesetId;
        public bool allowLose;
        public bool forceWinBias;
        public string designerNote;
        public string mechanic;
        public ObjectiveJsonDto[] goals;
        public string[] boardRows;
    }

    [Serializable]
    internal sealed class ObjectiveJsonDto
    {
        public string type;
        public int target;
        public int targetTileType = -1;
    }
}
