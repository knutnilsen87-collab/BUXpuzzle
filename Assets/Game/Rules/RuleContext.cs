namespace Game.Rules
{
    // Thin adapter context. Wire to real systems later.
    public class RuleContext
    {
        public int TurnIndex { get; set; }
        public int Seed { get; set; }

        public IBoardApi Board { get; }
        public IFeedbackApi Feedback { get; }
        public IRewardsApi Rewards { get; }
        public ITelemetryApi Telemetry { get; }

        public ResolveSummary LastResolve { get; set; }

        public RuleContext(IBoardApi board, IFeedbackApi feedback, IRewardsApi rewards, ITelemetryApi telemetry)
        {
            Board = board;
            Feedback = feedback;
            Rewards = rewards;
            Telemetry = telemetry;
        }
    }

    public struct ResolveSummary
    {
        public int merges;
        public int combo;
        public bool hadMatch;
    }

    public interface IBoardApi
    {
        bool TryMakeRandomTileWild(out int x, out int y);
    }

    public interface IFeedbackApi
    {
        void Emit(string eventName);
    }

    public interface IRewardsApi
    {
        void AddSoft(int amount, string reason);
    }

    public interface ITelemetryApi
    {
        void Event(string name, object payload = null);
    }
}