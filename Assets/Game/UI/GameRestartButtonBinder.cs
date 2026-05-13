using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Automatically connects UI Buttons to GameRoot restart methods.
    ///
    /// Rename your Unity buttons to one of these names:
    ///
    /// Current level restart:
    /// - RestartButton
    /// - RestartLevelButton
    /// - TryAgainButton
    /// - ReplayButton
    /// - SpillIgjenButton
    ///
    /// Full game reset:
    /// - ResetProgressButton
    /// - FullResetButton
    /// - NewGameButton
    /// - StartOverButton
    ///
    /// This avoids manually editing Button -> OnClick in the Inspector.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class GameRestartButtonBinder : MonoBehaviour
    {
        private static readonly string[] RestartCurrentLevelButtonNames =
        {
            "RestartButton",
            "Restart Level Button",
            "RestartLevelButton",
            "TryAgainButton",
            "Try Again Button",
            "ReplayButton",
            "Replay Button",
            "SpillIgjenButton",
            "Spill Igjen Button"
        };

        private static readonly string[] FullResetButtonNames =
        {
            "ResetProgressButton",
            "Reset Progress Button",
            "FullResetButton",
            "Full Reset Button",
            "NewGameButton",
            "New Game Button",
            "StartOverButton",
            "Start Over Button"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<GameRestartButtonBinder>() != null)
            {
                return;
            }

            var go = new GameObject("GameRestartButtonBinder");
            DontDestroyOnLoad(go);
            go.AddComponent<GameRestartButtonBinder>();
        }

        private IEnumerator Start()
        {
            // Wait a few frames so runtime-created UI has time to appear.
            for (int i = 0; i < 10; i++)
            {
                BindButtons();
                yield return null;
            }
        }

        private void BindButtons()
        {
            var gameRoot = FindFirstObjectByType<GameRoot>();

            if (gameRoot == null)
            {
                return;
            }

            BindNamedButtons(RestartCurrentLevelButtonNames, gameRoot.RestartCurrentLevel, "RestartCurrentLevel");
            BindNamedButtons(FullResetButtonNames, gameRoot.RestartWholeGameFromLevelOne, "RestartWholeGameFromLevelOne");
        }

        private static void BindNamedButtons(string[] names, UnityEngine.Events.UnityAction action, string methodName)
        {
            foreach (string buttonName in names)
            {
                var go = GameObject.Find(buttonName);

                if (go == null)
                {
                    continue;
                }

                var button = go.GetComponent<Button>();

                if (button == null)
                {
                    Debug.LogWarning($"[GameRestartButtonBinder] Found '{buttonName}', but it has no UnityEngine.UI.Button component.");
                    continue;
                }

                button.onClick.RemoveListener(action);
                button.onClick.AddListener(action);

                Debug.Log($"[GameRestartButtonBinder] Connected '{buttonName}' -> GameRoot.{methodName}()");
            }
        }
    }
}
