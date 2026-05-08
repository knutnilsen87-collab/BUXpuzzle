using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class RuleRuntimeTools
    {
        [MenuItem("FBL/Rules/Add RuleRuntime to Scene")]
        public static void AddRuleRuntime()
        {
            var go = GameObject.Find("RuleRuntime");
            if (go == null) go = new GameObject("RuleRuntime");
            if (go.GetComponent<Game.Rules.Unity.RuleRuntime>() == null)
                go.AddComponent<Game.Rules.Unity.RuleRuntime>();

            Selection.activeGameObject = go;
            Debug.Log("[FBL] RuleRuntime added/selected. Wire NotifyTurnStart/NotifyAfterResolve from your controller when ready.");
        }
    }
}