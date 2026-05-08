#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Presentation.NatureLight;

public static class FBL_GenerateNatureLightTiles
{
public static void Execute()
{
string prefabDir="Assets/Game/Presentation/NatureLight/Prefabs";
if(!AssetDatabase.IsValidFolder(prefabDir))
AssetDatabase.CreateFolder("Assets/Game/Presentation/NatureLight","Prefabs");

GameObject tile=new GameObject("NatureLightTile");

var renderer=tile.AddComponent<SpriteRenderer>();

var binder=tile.AddComponent<FBL_TileVisualBinder>();

CreateSymbol(tile,"Leaf");
CreateSymbol(tile,"Drop");
CreateSymbol(tile,"Sun");
CreateSymbol(tile,"Flower");
CreateSymbol(tile,"Crystal");
CreateSymbol(tile,"Berry");

string prefabPath=prefabDir+"/NatureLightTile.prefab";
PrefabUtility.SaveAsPrefabAsset(tile,prefabPath);

Object.DestroyImmediate(tile);

Debug.Log("Nature Light tile prefab created.");

EditorApplication.Exit(0);
}

static void CreateSymbol(GameObject parent,string name)
{
var go=new GameObject(name+"Symbol");
go.transform.SetParent(parent.transform,false);
var r=go.AddComponent<SpriteRenderer>();
}
}
#endif
