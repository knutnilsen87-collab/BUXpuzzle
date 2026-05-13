using UnityEngine;
using UnityEngine.UI;
using Game.Progression;

namespace Game.Presentation.UI
{
    public sealed class ResultsScreen : MonoBehaviour
    {
        [Header("Header")]
        public Text Headline;
        public Text Explanation;
        public Text Score;
        public Text Medal;
        public Text MovesLeft;
        public Text BestCascade;
        public Text ShareCode;

        [Header("Cards")]
        public RewardCard InstantRewardCard;
        public RewardCard XpCard;
        public RewardCard FragmentCard;
        public RewardCard SkillCard;

        public void Show(ResultsData data)
        {
            if (data == null) return;
            gameObject.SetActive(true);

            if (Headline != null) Headline.text = data.Headline ?? string.Empty;
            if (Explanation != null) Explanation.text = data.Explanation ?? string.Empty;
            if (Score != null) Score.text = data.Score.ToString();
            if (Medal != null) Medal.text = data.Medal.ToString() + "  " + data.Stars + "/3";
            if (MovesLeft != null) MovesLeft.text = data.MovesLeft + " trekk igjen";
            if (BestCascade != null) BestCascade.text = "Beste cascade: " + data.BestCascade;
            if (ShareCode != null) ShareCode.text = data.ShareCode ?? string.Empty;

            if (InstantRewardCard != null) InstantRewardCard.Hide();
            if (XpCard != null) XpCard.Hide();
            if (FragmentCard != null) FragmentCard.Hide();
            if (SkillCard != null) SkillCard.Hide();

            if (data.Rewards != null)
            {
                foreach (var r in data.Rewards)
                {
                    if (r.Currency == "token" && InstantRewardCard != null)
                        InstantRewardCard.Show("Token", r.Amount.ToString());
                    else if (r.Currency == "xp" && XpCard != null)
                        XpCard.Show("XP", "+" + r.Amount);
                    else if (r.Currency == "fragment" && FragmentCard != null)
                        FragmentCard.Show("Fragment", "+" + r.Amount);
                }
            }

            if (SkillCard != null)
            {
                if (!string.IsNullOrEmpty(data.SkillHighlight))
                    SkillCard.Show("Skill", data.SkillHighlight);
                else
                    SkillCard.Show("Tip", "Se etter større kjeder");
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
