namespace Game.Presentation
{
    public enum FeedbackEventType
    {
        Match,
        Cascade,
        Special,
        SmartMove,
        FailSoft,
        Reward
    }

    public struct FeedbackEvent
    {
        public FeedbackEventType Type;
        public int Intensity; // 1–5
    }
}
