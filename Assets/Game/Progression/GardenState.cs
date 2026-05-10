using UnityEngine;

namespace Game.Progression
{
    public sealed class GardenState
    {
        private const string NodesKey = "BUX_Save_GardenNodes";
        private const string LastUnlockKey = "BUX_Save_GardenLastUnlock";

        public int UnlockedNodes;
        public string LastUnlock;

        public static GardenState Load()
        {
            return new GardenState
            {
                UnlockedNodes = Mathf.Max(0, PlayerPrefs.GetInt(NodesKey, 0)),
                LastUnlock = PlayerPrefs.GetString(LastUnlockKey, string.Empty)
            };
        }

        public string ApplyFragments(int totalFragments)
        {
            int targetNodes = Mathf.Clamp(totalFragments / 5, 0, 12);
            if (targetNodes <= UnlockedNodes)
            {
                return NextUnlockText(totalFragments);
            }

            UnlockedNodes = targetNodes;
            LastUnlock = "Garden node " + UnlockedNodes;
            Save();
            return "Unlocked " + LastUnlock;
        }

        public string NextUnlockText(int totalFragments)
        {
            int nextAt = (UnlockedNodes + 1) * 5;
            int remaining = Mathf.Max(0, nextAt - totalFragments);
            return remaining == 0 ? "Garden ready" : remaining + " fragments to next garden node";
        }

        public void Save()
        {
            PlayerPrefs.SetInt(NodesKey, Mathf.Max(0, UnlockedNodes));
            PlayerPrefs.SetString(LastUnlockKey, LastUnlock ?? string.Empty);
            PlayerPrefs.Save();
        }
    }
}
