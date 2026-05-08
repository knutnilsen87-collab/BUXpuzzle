using System.Collections.Generic;

namespace Game.Progression
{
    public sealed class ResultsData
    {
        public bool Win;
        public string Headline;        // \"Nesten!\" / \"Bra jobbet!\"
        public string Explanation;     // \"Du manglet 2 blockers\"
        public List<RewardGrant> Rewards = new();
        public string SkillHighlight;  // Smart move / Tip learned
    }
}
