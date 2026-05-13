using Game.Levels;

namespace Game.Progression
{
    public sealed class ProgressionService
    {
        private readonly PlayerSave _save;
        private readonly LevelRepository _levels;

        public ProgressionService(PlayerSave save)
        {
            _save = save ?? PlayerSave.Load();
            _levels = new LevelRepository();
        }

        public LevelSpec CurrentLevel()
        {
            return _levels.GetLegacyLevel(_save.CurrentLevel);
        }

        public LevelSpecV2 CurrentLevelV2()
        {
            return _levels.GetLevel(_save.CurrentLevel);
        }

        public LevelSpec AdvanceLevel()
        {
            _save.AdvanceLevel();
            _save.Save();
            return CurrentLevel();
        }
    }
}
