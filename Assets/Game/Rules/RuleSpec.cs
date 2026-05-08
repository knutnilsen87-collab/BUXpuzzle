using System;

namespace Game.Rules
{
    [Serializable]
    public class RuleSpec
    {
        public RuleId id;
        public int durationTurns = 5;
        public int weight = 100;
    }

    [Serializable]
    public class ActiveRule
    {
        public RuleId id;
        public int turnsLeft;

        public ActiveRule(RuleId id, int durationTurns)
        {
            this.id = id;
            this.turnsLeft = durationTurns;
        }
    }
}