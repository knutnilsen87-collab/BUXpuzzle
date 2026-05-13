using System.Collections.Generic;

namespace Game.Progression
{
    public sealed class ResultsData
    {
        public bool Win;
        public string Headline;        // \"Nesten!\" / \"Bra jobbet!\"
        public string Explanation;     // \"Du manglet 2 blockers\"
        public int Score;
        public int Stars;
        public Medal Medal;
        public int MovesLeft;
        public int BestCascade;
        public string ShareCode;
        public string NextUnlock;
        public string GardenProgress;
        public List<RewardGrant> Rewards = new();
        public string SkillHighlight;  // Smart move / Tip learned
    }
}
