namespace Game.Progression
{
    public static class ShareCodeService
    {
        public static string Build(SessionResult result)
        {
            if (result == null) return string.Empty;
            return "BUX-" + result.RulesetId + "-L" + result.LevelId + "-S" + result.Seed + "-V1";
        }
    }
}
