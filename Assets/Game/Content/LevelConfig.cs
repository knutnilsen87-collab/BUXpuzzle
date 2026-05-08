using UnityEngine;

namespace Game.Content
{
    public enum GoalType { Score, ClearBlockers, CreateSpecials }

    [System.Serializable]
    public struct LevelGoal
    {
        public GoalType Type;
        public int Target;
    }

    [CreateAssetMenu(menuName = "BUX/LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        public int LevelId;
        public int WorldId;
        public int Width = 8;
        public int Height = 8;
        public int MoveLimit = 18;
        public LevelGoal[] Goals;

        [Header("Onboarding helpers")]
        public bool ForceWinBias;
        public bool AllowLose;
        public string DesignerNote;
    }
}
