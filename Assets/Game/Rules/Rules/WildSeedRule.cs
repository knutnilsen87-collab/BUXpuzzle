namespace Game.Rules.Rules
{
    public class WildSeedRule : IRule
    {
        public RuleId Id => RuleId.WildSeed;
        public string DisplayKey => "rule_wild_seed";

        public bool CanTrigger(RuleContext ctx) => true;

        public void OnTurnStart(RuleContext ctx)
        {
            if (ctx.Board != null && ctx.Board.TryMakeRandomTileWild(out var x, out var y))
            {
                ctx.Feedback?.Emit("rule_wild_seed");
                ctx.Telemetry?.Event("rule_triggered", new { id = Id.ToString(), x, y });
            }
        }

        public void OnAfterResolve(RuleContext ctx) { }
        public void OnExpire(RuleContext ctx) { }
    }
}