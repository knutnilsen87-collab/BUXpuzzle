#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using Game.Presentation.NatureLight;

public static class FBL_BindNatureLightTheme
{
    public static void Execute()
    {
        var theme = AssetDatabase.LoadAssetAtPath<FBL_NatureLightTheme>(
            "Assets/Game/Presentation/NatureLight/Resources/FBL_NatureLightTheme.asset");

        if (theme == null)
        {
            Debug.LogError("Theme asset missing");
            EditorApplication.Exit(1);
            return;
        }

        var root = GameObject.Find("GameRoot");

        if (root == null)
        {
            root = new GameObject("GameRoot");
        }

        var bootstrap = root.GetComponent<FBL_ThemeBootstrap>();

        if (bootstrap == null)
        {
            bootstrap = root.AddComponent<FBL_ThemeBootstrap>();
        }

        var so = new SerializedObject(bootstrap);
        var themeProp = so.FindProperty("theme");
        themeProp.objectReferenceValue = theme;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Theme bound to bootstrap.");
        EditorApplication.Exit(0);
    }
}
#endif

