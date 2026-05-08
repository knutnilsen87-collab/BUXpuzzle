using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game.Levels
{
    [DefaultExecutionOrder(-900)]
    public sealed class EndlessLevelDirector : MonoBehaviour
    {
        [SerializeField] private int startLevelIndex = 1;
        [SerializeField] private int boardWidth = 8;
        [SerializeField] private int boardHeight = 8;
        [SerializeField] private int tileKindCount = 6;
        [SerializeField] private bool persistLevelProgress = true;

        public int CurrentLevelIndex { get; private set; }
        public int CurrentSeed { get; private set; }

        private const string LevelKey = "FBL_Endless_CurrentLevel";

        private void Awake()
        {
            CurrentLevelIndex = persistLevelProgress ? Math.Max(1, PlayerPrefs.GetInt(LevelKey, startLevelIndex)) : startLevelIndex;
            CurrentSeed = DeterministicEndlessLevelGenerator.SeedForLevel(CurrentLevelIndex);
            TryBridgeIntoRuntime(CurrentLevelIndex, CurrentSeed);
            Debug.Log($"[FBL] EndlessLevelDirector ready. level={CurrentLevelIndex} seed={CurrentSeed}");
        }

        public void AdvanceLevel()
        {
            CurrentLevelIndex++;
            CurrentSeed = DeterministicEndlessLevelGenerator.SeedForLevel(CurrentLevelIndex);
            if (persistLevelProgress)
            {
                PlayerPrefs.SetInt(LevelKey, CurrentLevelIndex);
                PlayerPrefs.Save();
            }
            TryBridgeIntoRuntime(CurrentLevelIndex, CurrentSeed);
            Debug.Log($"[FBL] EndlessLevelDirector advanced. level={CurrentLevelIndex} seed={CurrentSeed}");
        }

        public int[,] PreviewBoardForCurrentLevel()
        {
            return DeterministicEndlessLevelGenerator.GenerateBoard(boardWidth, boardHeight, tileKindCount, CurrentLevelIndex);
        }

        private void TryBridgeIntoRuntime(int levelIndex, int seed)
        {
            try
            {
                var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

                foreach (var mb in monoBehaviours.Where(x => x != null))
                {
                    var t = mb.GetType();

                    // Try common seed/level fields.
                    SetFieldIfExists(mb, t, "seed", seed);
                    SetFieldIfExists(mb, t, "currentSeed", seed);
                    SetFieldIfExists(mb, t, "levelIndex", levelIndex);
                    SetFieldIfExists(mb, t, "currentLevelIndex", levelIndex);

                    // Try common setter methods.
                    InvokeIfExists(mb, t, "SetSeed", seed);
                    InvokeIfExists(mb, t, "ConfigureSeed", seed);
                    InvokeIfExists(mb, t, "SetLevelIndex", levelIndex);
                    InvokeIfExists(mb, t, "ConfigureLevel", levelIndex);

                    // Try combined signatures.
                    InvokeIfExists(mb, t, "ConfigureLevelAndSeed", levelIndex, seed);
                    InvokeIfExists(mb, t, "SetLevelAndSeed", levelIndex, seed);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FBL] EndlessLevelDirector bridge warning: " + ex.Message);
            }
        }

        private static void SetFieldIfExists(object instance, Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return;
            if (!field.FieldType.IsAssignableFrom(value.GetType())) return;

            field.SetValue(instance, value);
        }

        private static void InvokeIfExists(object instance, Type type, string methodName, params object[] args)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                              .Where(m => m.Name == methodName)
                              .ToArray();

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != args.Length) continue;

                bool compatible = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                    {
                        compatible = false;
                        break;
                    }
                }

                if (!compatible) continue;
                method.Invoke(instance, args);
                return;
            }
        }

        private void Start()
        {
            var gameRoot = UnityEngine.Object.FindFirstObjectByType<GameRoot>();
            if (gameRoot != null)
            {
                gameRoot.ConfigureRuntimeSeed(CurrentSeed, CurrentLevelIndex);
            }
        }
    }
}


