#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class FBL_BindTilePrefab
{
public static void Execute()
{
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(
"Assets/Game/Presentation/NatureLight/Prefabs/NatureLightTile.prefab");

if(prefab==null)
{
Debug.LogError("Tile prefab missing");
EditorApplication.Exit(1);
return;
}

var renderer=Object.FindFirstObjectByType(
System.Type.GetType("FBL_DeterministicBoardRenderer"));

if(renderer==null)
{
Debug.LogError("Board renderer not found");
EditorApplication.Exit(1);
return;
}

var so=new SerializedObject(renderer);
var prop=so.FindProperty("tilePrefab");

if(prop!=null)
{
prop.objectReferenceValue=prefab;
so.ApplyModifiedPropertiesWithoutUndo();
}

EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

Debug.Log("Tile prefab bound to renderer.");

EditorApplication.Exit(0);
}
}
#endif
