namespace Game.Rules
{
    public interface IRule
    {
        RuleId Id { get; }
        string DisplayKey { get; } // key for icon/visual mapping (no gameplay text needed)
        bool CanTrigger(RuleContext ctx);
        void OnTurnStart(RuleContext ctx);
        void OnAfterResolve(RuleContext ctx);
        void OnExpire(RuleContext ctx);
    }
}