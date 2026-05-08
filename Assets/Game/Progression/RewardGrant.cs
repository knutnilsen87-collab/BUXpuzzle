namespace Game.Progression
{
    public enum RewardLayer
    {
        Sensoric = 1,
        Progress = 2,
        Mastery  = 3,
        Meta     = 4
    }

    public struct RewardGrant
    {
        public RewardLayer Layer;
        public string Currency; // xp, fragment, token, cosmetic
        public int Amount;
        public string Reason;   // guaranteed, smart_move, milestone
    }
}
