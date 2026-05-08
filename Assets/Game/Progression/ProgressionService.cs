using Game.Levels;

namespace Game.Progression
{
    public sealed class ProgressionService
    {
        private readonly PlayerSave _save;

        public ProgressionService(PlayerSave save)
        {
            _save = save ?? PlayerSave.Load();
        }

        public LevelSpec CurrentLevel()
        {
            return LevelManager.GetLevel(_save.CurrentLevel);
        }

        public LevelSpec AdvanceLevel()
        {
            _save.AdvanceLevel();
            _save.Save();
            return CurrentLevel();
        }
    }
}
