using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FBL_AssignTilePrefab
{
    public static void Run()
    {
        string prefabPath="Assets/Resources/NatureLightTile.prefab";

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if(prefab==null)
        {
            var go=new GameObject("NatureLightTile");
            var sr=go.AddComponent<SpriteRenderer>();

            Texture2D tex=new Texture2D(32,32);

            for(int x=0;x<32;x++)
            for(int y=0;y<32;y++)
                tex.SetPixel(x,y,Color.green);

            tex.Apply();

            sr.sprite=Sprite.Create(tex,new Rect(0,0,32,32),new Vector2(.5f,.5f));

            prefab=PrefabUtility.SaveAsPrefabAsset(go,prefabPath);

            Object.DestroyImmediate(go);
        }

        var scene=EditorSceneManager.OpenScene("Assets/Scenes/game.unity");

        var boardView=Object.FindObjectOfType<Game.Presentation.BoardView>();

        var so=new SerializedObject(boardView);

        so.FindProperty("natureLightTilePrefab").objectReferenceValue=prefab;

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("NatureLightTile assigned to BoardView.");
    }
}
