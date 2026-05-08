using UnityEditor;
using UnityEngine;

public static class FBL_GenerateNatureLightTilePrefab
{
    public static void Run()
    {
        const string prefabPath = "Assets/Game/Presentation/NatureLight/NatureLightTile.prefab";

        var root = new GameObject("NatureLightTile");

        var baseRenderer = root.AddComponent<SpriteRenderer>();

        var symbol = new GameObject("Symbol");
        symbol.transform.SetParent(root.transform,false);

        var symbolRenderer = symbol.AddComponent<SpriteRenderer>();
        symbolRenderer.sortingOrder = 1;

        PrefabUtility.SaveAsPrefabAsset(root,prefabPath);

        Object.DestroyImmediate(root);

        Debug.Log("NatureLightTile prefab generated.");
    }
}
