using UnityEditor;
using UnityEngine;
using System.IO;
using Game.Config;
using Game.Rules;

public static class FBLRuleAssetTools
{
    private const string FlagsDir = "Assets/Game/Config";
    private const string RulesDir = "Assets/Game/Rules/Defs";

    [MenuItem("FBL/Rules/Create Default Rule Assets")]
    public static void CreateDefaultRuleAssets()
    {
        Directory.CreateDirectory(FlagsDir);
        Directory.CreateDirectory(RulesDir);

        // 1) Feature flags asset
        var flagsPath = Path.Combine(FlagsDir, "FeatureFlags.asset").Replace("\\", "/");
        var flags = AssetDatabase.LoadAssetAtPath<FeatureFlagsSO>(flagsPath);
        if (flags == null)
        {
            flags = ScriptableObject.CreateInstance<FeatureFlagsSO>();
            flags.rules_enabled = true;
            flags.rules_mutation_enabled = false;
            flags.social_enabled = false;
            flags.rule_text_enabled = false;
            flags.snapshot_sampling_rate = 0.0f;

            AssetDatabase.CreateAsset(flags, flagsPath);
            Debug.Log("[FBL] Created FeatureFlags asset: " + flagsPath);
        }
        else
        {
            flags.rules_enabled = true;
            EditorUtility.SetDirty(flags);
            Debug.Log("[FBL] Updated FeatureFlags asset (rules_enabled=true): " + flagsPath);
        }

        // 2) Rule defs
        var combo = EnsureRuleDef(
            "ComboBonus.asset",
            RuleId.ComboBonus,
            durationTurns: 5,
            visualSig: "vfx_rule_combo_bonus",
            audioSig: "sfx_rule_combo_bonus",
            paramInt: 1,
            paramFloat: 0f
        );

        var wild = EnsureRuleDef(
            "WildSeed.asset",
            RuleId.WildSeed,
            durationTurns: 5,
            visualSig: "vfx_rule_wild_seed",
            audioSig: "sfx_rule_wild_seed",
            paramInt: 0,
            paramFloat: 0f
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 3) Auto-assign to RuleRuntime if present in scene
        TryAssignToRuleRuntime(flags, new[] { combo, wild });

        Debug.Log("[FBL] Default rule assets ready.");
    }

    private static RuleDefinitionSO EnsureRuleDef(
        string fileName,
        RuleId id,
        int durationTurns,
        string visualSig,
        string audioSig,
        int paramInt,
        float paramFloat)
    {
        var path = Path.Combine(RulesDir, fileName).Replace("\\", "/");
        var def = AssetDatabase.LoadAssetAtPath<RuleDefinitionSO>(path);
        if (def == null)
        {
            def = ScriptableObject.CreateInstance<RuleDefinitionSO>();
            def.id = id;
            def.durationTurns = durationTurns;
            def.visualSignature = visualSig;
            def.audioSignature = audioSig;
            def.paramInt = paramInt;
            def.paramFloat = paramFloat;

            AssetDatabase.CreateAsset(def, path);
            Debug.Log("[FBL] Created RuleDefinition: " + path);
        }
        else
        {
            def.id = id;
            def.durationTurns = durationTurns;
            def.visualSignature = visualSig;
            def.audioSignature = audioSig;
            def.paramInt = paramInt;
            def.paramFloat = paramFloat;

            EditorUtility.SetDirty(def);
            Debug.Log("[FBL] Updated RuleDefinition: " + path);
        }

        return def;
    }

    private static void TryAssignToRuleRuntime(FeatureFlagsSO flags, RuleDefinitionSO[] defs)
    {
        // Look for RuleRuntime in scene
        var runtimeType = System.Type.GetType("Game.Rules.Unity.RuleRuntime, Game.Rules")
                         ?? System.Type.GetType("Game.Rules.Unity.RuleRuntime");
        if (runtimeType == null)
        {
            Debug.LogWarning("[FBL] RuleRuntime type not found. Assign manually later.");
            return;
        }

        var obj = Object.FindFirstObjectByType(runtimeType) as Component;
        if (obj == null)
        {
            Debug.LogWarning("[FBL] RuleRuntime not found in scene. Assign manually later.");
            return;
        }

        var so = new SerializedObject(obj);
        var pFlags = so.FindProperty("FeatureFlags");
        if (pFlags != null) pFlags.objectReferenceValue = flags;

        var pDefs = so.FindProperty("RuleDefinitions");
        if (pDefs != null && pDefs.isArray)
        {
            pDefs.arraySize = defs.Length;
            for (int i = 0; i < defs.Length; i++)
            {
                var el = pDefs.GetArrayElementAtIndex(i);
                el.objectReferenceValue = defs[i];
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(obj.gameObject);
        Debug.Log("[FBL] Assigned FeatureFlags + RuleDefinitions to RuleRuntime in scene.");
    }
}