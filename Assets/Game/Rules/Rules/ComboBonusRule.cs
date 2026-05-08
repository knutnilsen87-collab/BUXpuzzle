namespace Game.Rules.Rules
{
    public class ComboBonusRule : IRule
    {
        public RuleId Id => RuleId.ComboBonus;
        public string DisplayKey => "rule_combo_bonus";

        public bool CanTrigger(RuleContext ctx) => true;

        public void OnTurnStart(RuleContext ctx) { }

        public void OnAfterResolve(RuleContext ctx)
        {
            if (ctx.LastResolve.hadMatch && ctx.LastResolve.combo >= 2)
            {
                ctx.Rewards?.AddSoft(1, "rule_combo_bonus");
                ctx.Feedback?.Emit("combo_bonus");
                ctx.Telemetry?.Event("rule_triggered", new { id = Id.ToString(), combo = ctx.LastResolve.combo });
            }
        }

        public void OnExpire(RuleContext ctx) { }
    }
}