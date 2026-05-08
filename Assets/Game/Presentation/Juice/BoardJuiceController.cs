using Game.Audio;
using Game.Core;
using UnityEngine;

namespace Game.Presentation.Juice
{
    public sealed class BoardJuiceController : MonoBehaviour
    {
        private static BoardJuiceController _instance;
        private MatchRewardPresenter _rewardPresenter;
        private ScreenPulse _screenPulse;

        public static BoardJuiceController Ensure()
        {
            if (_instance != null) return _instance;
            var existing = FindFirstObjectByType<BoardJuiceController>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }

            var go = new GameObject("BoardJuiceController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<BoardJuiceController>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            _rewardPresenter = GetComponent<MatchRewardPresenter>();
            if (_rewardPresenter == null) _rewardPresenter = gameObject.AddComponent<MatchRewardPresenter>();
            _screenPulse = GetComponent<ScreenPulse>();
            if (_screenPulse == null) _screenPulse = gameObject.AddComponent<ScreenPulse>();
        }

        public void TileSelected()
        {
            GameAudioController.Ensure().Play(AudioEvent.TileSelect);
            HapticsController.Light();
        }

        public void TileDeselected()
        {
            GameAudioController.Ensure().Play(AudioEvent.TileDeselect, 0.65f);
        }

        public void SwapAccepted()
        {
            GameAudioController.Ensure().Play(AudioEvent.SwapAccept);
        }

        public void SwapRejected()
        {
            GameAudioController.Ensure().Play(AudioEvent.SwapReject);
            HapticsController.Warning();
        }

        public void MatchFound(ResolveStep step)
        {
            int count = step != null && step.Matched != null ? step.Matched.Count : 3;
            var evt = count >= 5 ? AudioEvent.Match5 : (count == 4 ? AudioEvent.Match4 : AudioEvent.Match3);
            GameAudioController.Ensure().Play(evt);
        }

        public void Clear()
        {
            GameAudioController.Ensure().Play(AudioEvent.Clear, 0.85f);
        }

        public void DropLand()
        {
            GameAudioController.Ensure().Play(AudioEvent.DropLand, 0.8f);
        }

        public void Cascade(int iteration)
        {
            if (iteration <= 1)
            {
                return;
            }

            GameAudioController.Ensure().Play(iteration == 2 ? AudioEvent.Cascade2 : AudioEvent.Cascade3Plus, 0.78f);
            if (_rewardPresenter != null) _rewardPresenter.ShowCascade(iteration);
        }

        public void LevelComplete()
        {
            GameAudioController.Ensure().Play(AudioEvent.LevelComplete);
            HapticsController.Success();
            if (_screenPulse != null) _screenPulse.Pulse(0.38f);
        }
    }
}
