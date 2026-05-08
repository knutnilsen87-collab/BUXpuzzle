using UnityEditor;
using UnityEngine;
using System.IO;

public static class FBL_VerifyOrCreateNatureLightPrefab
{
    public static void Run()
    {
        string path = "Assets/Resources/NatureLightTile.prefab";

        if (File.Exists(path))
        {
            Debug.Log("Prefab already exists.");
            return;
        }

        Debug.Log("Creating NatureLightTile prefab...");

        GameObject go = new GameObject("NatureLightTile");

        var sr = go.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(32,32);

        for(int x=0;x<32;x++)
        for(int y=0;y<32;y++)
            tex.SetPixel(x,y,Color.green);

        tex.Apply();

        var sprite = Sprite.Create(
            tex,
            new Rect(0,0,32,32),
            new Vector2(0.5f,0.5f)
        );

        sr.sprite = sprite;

        PrefabUtility.SaveAsPrefabAsset(go,path);

        Object.DestroyImmediate(go);

        Debug.Log("Prefab created.");
    }
}
