using System.Collections.Generic;

namespace Game.Progression
{
    public sealed class RewardPipeline
    {
        // ALWAYS grants something, even on loss
        public ResultsData Compute(
            bool win,
            int secondsPlayed,
            int movesUsed,
            int goalsRemaining,
            bool smartMoveTriggered)
        {
            var results = new ResultsData();
            results.Win = win;

            // Header copy
            if (win)
            {
                results.Headline = "Bra jobbet!";
                results.Explanation = "Du fullførte målene";
            }
            else
            {
                results.Headline = "Nesten!";
                results.Explanation = goalsRemaining > 0
                    ? ("Du manglet " + goalsRemaining)
                    : "Du var veldig nær";
            }

            // Layer 2: Guaranteed progress
            int xp = 10 + (secondsPlayed / 5) + (movesUsed / 2);
            if (xp < 15) xp = 15;

            results.Rewards.Add(new RewardGrant
            {
                Layer = RewardLayer.Progress,
                Currency = "xp",
                Amount = xp,
                Reason = "guaranteed"
            });

            results.Rewards.Add(new RewardGrant
            {
                Layer = RewardLayer.Progress,
                Currency = "fragment",
                Amount = 1,
                Reason = "guaranteed"
            });

            results.Rewards.Add(new RewardGrant
            {
                Layer = RewardLayer.Progress,
                Currency = "token",
                Amount = 1,
                Reason = "guaranteed"
            });

            // Layer 3: Mastery (optional)
            if (smartMoveTriggered)
            {
                results.Rewards.Add(new RewardGrant
                {
                    Layer = RewardLayer.Mastery,
                    Currency = "xp",
                    Amount = 10,
                    Reason = "smart_move"
                });

                results.Rewards.Add(new RewardGrant
                {
                    Layer = RewardLayer.Mastery,
                    Currency = "fragment",
                    Amount = 1,
                    Reason = "smart_move"
                });

                results.SkillHighlight = "Smart trekk!";
            }
            else
            {
                results.SkillHighlight = "Tips: Se etter større kjeder";
            }

            return results;
        }
    }
}
