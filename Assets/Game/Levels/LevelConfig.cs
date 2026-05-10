using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "BUXPuzzle/Level Config")]
    public sealed class LevelConfig : ScriptableObject
    {
        public int levelNumber = 1;
        public int goalMatches = 10;
        public int moveLimit = 13;
        public int boardWidth = 8;
        public int boardHeight = 8;
    }
}
