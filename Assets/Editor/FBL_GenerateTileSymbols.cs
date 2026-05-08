#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FBL_GenerateTileSymbols
{
public static void Execute()
{
string dir="Assets/Game/Presentation/NatureLight/Sprites";

if(!AssetDatabase.IsValidFolder(dir))
AssetDatabase.CreateFolder("Assets/Game/Presentation/NatureLight","Sprites");

Create("Leaf");
Create("Drop");
Create("Sun");
Create("Flower");
Create("Crystal");
Create("Berry");

AssetDatabase.SaveAssets();
AssetDatabase.Refresh();

Debug.Log("Tile symbols generated.");

EditorApplication.Exit(0);
}

static void Create(string name)
{
Texture2D tex=new Texture2D(128,128);

for(int x=0;x<128;x++)
for(int y=0;y<128;y++)
{
tex.SetPixel(x,y,Color.white);
}

tex.Apply();

var bytes=tex.EncodeToPNG();

var path="Assets/Game/Presentation/NatureLight/Sprites/"+name+".png";

System.IO.File.WriteAllBytes(path,bytes);
}
}
#endif
