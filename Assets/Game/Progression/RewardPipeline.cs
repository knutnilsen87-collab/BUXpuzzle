using System.Collections.Generic;

namespace Game.Progression
{
    public sealed class RewardPipeline
    {
        public ResultsData Compute(SessionResult session)
        {
            if (session == null)
            {
                return Compute(false, 0, 0, 1, false);
            }

            ResultsData results = Compute(
                session.Win,
                0,
                session.MovesUsed,
                session.GoalsRemaining,
                session.BestCascade >= 2);

            results.Score = session.Score;
            results.Stars = session.Stars;
            results.Medal = session.Medal;
            results.MovesLeft = session.MovesLeft;
            results.BestCascade = session.BestCascade;
            results.ShareCode = session.ShareCode;
            session.Rewards = results.Rewards.ToArray();
            return results;
        }

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
