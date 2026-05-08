#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Presentation.NatureLight;

public static class FBL_CreateNatureLightThemeAsset
{
    public static void Execute()
    {
        const string folder = "Assets/Game/Presentation/NatureLight/Resources";
        const string assetPath = "Assets/Game/Presentation/NatureLight/Resources/FBL_NatureLightTheme.asset";

        if (!AssetDatabase.IsValidFolder("Assets/Game/Presentation/NatureLight"))
        {
            Debug.LogError("NatureLight folder missing.");
            EditorApplication.Exit(2);
            return;
        }

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Game/Presentation/NatureLight", "Resources");
        }

        var existing = AssetDatabase.LoadAssetAtPath<FBL_NatureLightTheme>(assetPath);
        if (existing == null)
        {
            var asset = ScriptableObject.CreateInstance<FBL_NatureLightTheme>();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created theme asset: " + assetPath);
        }
        else
        {
            Debug.Log("Theme asset already exists: " + assetPath);
        }

        EditorApplication.Exit(0);
    }
}
#endif
