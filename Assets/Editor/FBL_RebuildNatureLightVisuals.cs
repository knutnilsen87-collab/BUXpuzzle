#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Presentation.NatureLight;

public static class FBL_RebuildNatureLightVisuals
{
    private const int Size = 128;
    private const string SpriteDir = "Assets/Game/Presentation/NatureLight/Resources/Sprites";
    private const string PrefabDir = "Assets/Game/Presentation/NatureLight/Prefabs";
    private const string PrefabPath = "Assets/Game/Presentation/NatureLight/Prefabs/NatureLightTile.prefab";

    public static void Execute()
    {
        EnsureFolder("Assets/Game/Presentation/NatureLight", "Resources");
        EnsureFolder("Assets/Game/Presentation/NatureLight/Resources", "Sprites");
        EnsureFolder("Assets/Game/Presentation/NatureLight", "Prefabs");

        WriteSprite("Leaf_Base",    MakeTexture(DrawSoftHex));
        WriteSprite("Drop_Base",    MakeTexture(DrawCapsule));
        WriteSprite("Sun_Base",     MakeTexture(DrawRoundedDiamond));
        WriteSprite("Flower_Base",  MakeTexture(DrawRoundedSquare));
        WriteSprite("Crystal_Base", MakeTexture(DrawTallDiamond));
        WriteSprite("Berry_Base",   MakeTexture(DrawWideRoundedSquare));

        WriteSprite("Leaf_Symbol",    MakeTexture(DrawLeaf));
        WriteSprite("Drop_Symbol",    MakeTexture(DrawDrop));
        WriteSprite("Sun_Symbol",     MakeTexture(DrawSun));
        WriteSprite("Flower_Symbol",  MakeTexture(DrawFlower));
        WriteSprite("Crystal_Symbol", MakeTexture(DrawCrystal));
        WriteSprite("Berry_Symbol",   MakeTexture(DrawBerry));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var prefabRoot = new GameObject("NatureLightTile");
        var baseRenderer = prefabRoot.AddComponent<SpriteRenderer>();
        prefabRoot.AddComponent<FBL_TileVisualBinder>();

        var symbol = new GameObject("Symbol");
        symbol.transform.SetParent(prefabRoot.transform, false);
        symbol.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        symbol.AddComponent<SpriteRenderer>();

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        Object.DestroyImmediate(prefabRoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Nature Light visuals rebuilt.");
        EditorApplication.Exit(0);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Texture2D MakeTexture(System.Func<float, float, bool> inside)
    {
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                float nx = (x + 0.5f) / Size * 2f - 1f;
                float ny = (y + 0.5f) / Size * 2f - 1f;
                bool fill = inside(nx, ny);
                tex.SetPixel(x, y, fill ? Color.white : new Color(0f, 0f, 0f, 0f));
            }
        }

        tex.Apply();
        return tex;
    }

    private static void WriteSprite(string name, Texture2D tex)
    {
        string path = SpriteDir + "/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100f;
        importer.SaveAndReimport();
    }

    private static bool DrawRoundedSquare(float x, float y)
    {
        float hx = 0.68f;
        float hy = 0.68f;
        float r = 0.28f;
        return SdRoundedRect(x, y, hx, hy, r) <= 0f;
    }

    private static bool DrawWideRoundedSquare(float x, float y)
    {
        float hx = 0.76f;
        float hy = 0.62f;
        float r = 0.28f;
        return SdRoundedRect(x, y, hx, hy, r) <= 0f;
    }

    private static bool DrawCapsule(float x, float y)
    {
        float hx = 0.48f;
        float hy = 0.74f;
        float r = 0.42f;
        return SdRoundedRect(x, y, hx, hy, r) <= 0f;
    }

    private static bool DrawRoundedDiamond(float x, float y)
    {
        float d = Mathf.Abs(x) + Mathf.Abs(y) - 0.88f;
        float soften = 0.08f;
        return d <= soften;
    }

    private static bool DrawTallDiamond(float x, float y)
    {
        float d = Mathf.Abs(x) * 1.15f + Mathf.Abs(y) * 0.82f - 0.88f;
        float soften = 0.07f;
        return d <= soften;
    }

    private static bool DrawSoftHex(float x, float y)
    {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        return y <= 0.78f && (0.58f * x + y) <= 0.92f;
    }

    private static bool DrawLeaf(float x, float y)
    {
        float body = (x * x) / 0.18f + ((y + 0.02f) * (y + 0.02f)) / 0.34f;
        bool main = body <= 1f && y > -0.62f && Mathf.Abs(x) <= 0.45f;
        bool tipCut = y < 0.78f;
        bool vein = Mathf.Abs(x) < 0.05f && y > -0.52f && y < 0.52f;
        return (main && tipCut) || vein;
    }

    private static bool DrawDrop(float x, float y)
    {
        float circle = x * x + (y - 0.12f) * (y - 0.12f);
        bool lower = circle <= 0.28f;
        bool upper = Mathf.Abs(x) + (y + 0.42f) * 1.12f <= 0.56f && y <= 0.14f;
        return lower || upper;
    }

    private static bool DrawSun(float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        float a = Mathf.Atan2(y, x);
        float spikes = 0.18f * Mathf.Cos(a * 8f);
        return r <= 0.34f + spikes;
    }

    private static bool DrawFlower(float x, float y)
    {
        bool center = x * x + y * y <= 0.08f;
        bool p1 = (x + 0.22f) * (x + 0.22f) + y * y <= 0.07f;
        bool p2 = (x - 0.22f) * (x - 0.22f) + y * y <= 0.07f;
        bool p3 = x * x + (y + 0.22f) * (y + 0.22f) <= 0.07f;
        bool p4 = x * x + (y - 0.22f) * (y - 0.22f) <= 0.07f;
        return center || p1 || p2 || p3 || p4;
    }

    private static bool DrawCrystal(float x, float y)
    {
        bool body = Mathf.Abs(x) * 1.2f + Mathf.Abs(y) * 0.95f <= 0.58f;
        bool top = Mathf.Abs(x) <= 0.10f && y <= -0.42f;
        return body || top;
    }

    private static bool DrawBerry(float x, float y)
    {
        bool berry = x * x + y * y <= 0.18f;
        bool leaf = ((x + 0.18f) * (x + 0.18f)) / 0.05f + ((y - 0.20f) * (y - 0.20f)) / 0.02f <= 1f;
        return berry || leaf;
    }

    private static float SdRoundedRect(float x, float y, float hx, float hy, float r)
    {
        float qx = Mathf.Abs(x) - hx + r;
        float qy = Mathf.Abs(y) - hy + r;
        float ax = Mathf.Max(qx, 0f);
        float ay = Mathf.Max(qy, 0f);
        return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
    }
}
#endif
