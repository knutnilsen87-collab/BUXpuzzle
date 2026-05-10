using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Presentation
{
    public static class NatureLightRuntimeArt
    {
        private const int TileSize = 128;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private static readonly Color[] BaseColors =
        {
            new Color(0.35f, 0.72f, 0.45f, 1f),
            new Color(0.30f, 0.62f, 0.90f, 1f),
            new Color(0.96f, 0.79f, 0.28f, 1f),
            new Color(0.92f, 0.45f, 0.63f, 1f),
            new Color(0.62f, 0.50f, 0.88f, 1f),
            new Color(0.84f, 0.30f, 0.38f, 1f),
        };

        public static Sprite TileBase(int type, bool selected)
        {
            string key = "tile-base-" + Mathf.Clamp(type, 0, 5) + "-" + selected;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            Color baseColor = BaseColors[Mathf.Clamp(type, 0, 5)];
            Color top = Color.Lerp(baseColor, Color.white, selected ? 0.48f : 0.32f);
            Color bottom = Color.Lerp(baseColor, Color.black, selected ? 0.06f : 0.18f);
            Color rim = Color.Lerp(baseColor, Color.white, selected ? 0.70f : 0.42f);
            Color inner = Color.Lerp(baseColor, Color.white, selected ? 0.22f : 0.12f);

            var texture = CreateTexture(TileSize, TileSize, (x, y) =>
            {
                float px = (x + 0.5f) / TileSize;
                float py = (y + 0.5f) / TileSize;
                float edge = RoundedRectDistance(px, py, 0.50f, 0.50f, 0.42f, 0.42f, 0.18f);
                if (edge > 0.018f) return Clear();

                float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, edge) / 0.018f);
                Color c = Color.Lerp(bottom, top, py);

                if (edge > -0.045f) c = Color.Lerp(c, rim, 0.72f);
                if (py > 0.62f && edge < -0.09f) c = Color.Lerp(c, inner, 0.28f);

                float glow = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(px, py), new Vector2(0.32f, 0.76f)) / 0.36f);
                c = Color.Lerp(c, Color.white, glow * (selected ? 0.16f : 0.09f));
                c.a = alpha;
                return c;
            });

            return Store(key, texture);
        }

        public static Sprite TileSymbol(int type)
        {
            type = Mathf.Clamp(type, 0, 5);
            string key = "tile-symbol-" + type;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = CreateTexture(TileSize, TileSize, (x, y) =>
            {
                float px = ((x + 0.5f) / TileSize - 0.5f) * 2f;
                float py = ((y + 0.5f) / TileSize - 0.5f) * 2f;
                float mask = Mathf.Clamp01(SymbolMask(type, px, py));
                if (mask <= 0.01f) return Clear();

                Color c = new Color(1f, 0.98f, 0.88f, mask * 0.96f);
                if (IsSymbolLine(type, px, py))
                {
                    c = new Color(0.13f, 0.20f, 0.22f, mask * 0.35f);
                }

                return c;
            });

            return Store(key, texture);
        }

        public static Sprite TileShadow()
        {
            const string key = "tile-shadow";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = CreateTexture(TileSize, TileSize, (x, y) =>
            {
                float px = ((x + 0.5f) / TileSize - 0.5f) * 2f;
                float py = ((y + 0.5f) / TileSize - 0.5f) * 2f;
                float d = (px * px) / 0.88f + ((py + 0.14f) * (py + 0.14f)) / 0.36f;
                float alpha = Mathf.Clamp01((1f - d) * 0.18f);
                return new Color(0.02f, 0.06f, 0.07f, alpha);
            });

            return Store(key, texture);
        }

        public static Sprite CellSlot()
        {
            const string key = "cell-slot";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = CreateTexture(TileSize, TileSize, (x, y) =>
            {
                float px = (x + 0.5f) / TileSize;
                float py = (y + 0.5f) / TileSize;
                float edge = RoundedRectDistance(px, py, 0.50f, 0.50f, 0.43f, 0.43f, 0.16f);
                if (edge > 0.014f) return Clear();

                float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, edge) / 0.014f);
                Color top = new Color(0.55f, 0.70f, 0.58f, 0.28f);
                Color bottom = new Color(0.05f, 0.16f, 0.16f, 0.24f);
                Color c = Color.Lerp(bottom, top, py);

                if (edge > -0.035f) c = Color.Lerp(c, new Color(0.90f, 0.95f, 0.78f, 0.38f), 0.42f);
                if (edge < -0.24f) c = Color.Lerp(c, new Color(0.03f, 0.10f, 0.10f, 0.26f), 0.30f);
                float innerShade = Mathf.Clamp01((0.58f - py) * 0.70f);
                c = Color.Lerp(c, new Color(0.02f, 0.08f, 0.08f, 0.28f), innerShade * 0.22f);
                c.a *= alpha;
                return c;
            });

            return Store(key, texture);
        }

        public static Sprite BlockerFrame(CellBlockerType blocker)
        {
            string key = "blocker-frame-" + blocker;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            Color moss = new Color(0.35f, 0.86f, 0.26f, 1f);
            Color vine = new Color(0.24f, 0.66f, 0.18f, 1f);
            Color accent = blocker == CellBlockerType.Vine ? vine : moss;

            var texture = CreateTexture(TileSize, TileSize, (x, y) =>
            {
                float px = (x + 0.5f) / TileSize;
                float py = (y + 0.5f) / TileSize;
                float edge = RoundedRectDistance(px, py, 0.50f, 0.50f, 0.45f, 0.45f, 0.18f);
                float outer = Mathf.Clamp01(1f - Mathf.Abs(edge + 0.012f) / 0.030f);
                float innerGlow = Mathf.Clamp01(1f - Mathf.Abs(edge + 0.075f) / 0.045f) * 0.42f;
                float alpha = Mathf.Max(outer * 0.72f, innerGlow * 0.38f);
                if (alpha <= 0.01f) return Clear();

                Color c = Color.Lerp(accent, Color.white, py > 0.55f ? 0.18f : 0.02f);
                c.a = alpha;
                return c;
            });

            return Store(key, texture);
        }

        public static Sprite BoardPanel()
        {
            const string key = "board-panel";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = CreateTexture(192, 192, (x, y) =>
            {
                float px = (x + 0.5f) / 192f;
                float py = (y + 0.5f) / 192f;
                float edge = RoundedRectDistance(px, py, 0.50f, 0.50f, 0.46f, 0.46f, 0.11f);
                if (edge > 0.012f) return Clear();

                float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, edge) / 0.012f);
                Color innerBottom = new Color(0.24f, 0.43f, 0.39f, 0.96f);
                Color innerTop = new Color(0.66f, 0.80f, 0.60f, 0.94f);
                float radial = 1f - Mathf.Clamp01(Vector2.Distance(new Vector2(px, py), new Vector2(0.48f, 0.58f)) / 0.62f);
                Color c = Color.Lerp(innerBottom, innerTop, py);
                c = Color.Lerp(c, new Color(0.78f, 0.88f, 0.65f, 0.95f), radial * 0.18f);
                if (edge > -0.035f) c = Color.Lerp(c, new Color(0.96f, 0.90f, 0.66f, 1f), 0.48f);
                if (edge < -0.32f) c = Color.Lerp(c, new Color(0.18f, 0.34f, 0.34f, 0.94f), 0.12f);
                c.a *= alpha;
                return c;
            });

            return Store(key, texture, 100f);
        }

        public static Sprite Backdrop()
        {
            const string key = "backdrop";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = CreateTexture(64, 96, (x, y) =>
            {
                float px = (x + 0.5f) / 64f;
                float py = (y + 0.5f) / 96f;
                Color sky = Color.Lerp(new Color(0.47f, 0.71f, 0.83f, 1f), new Color(0.88f, 0.94f, 0.84f, 1f), py);
                Color garden = Color.Lerp(new Color(0.34f, 0.50f, 0.36f, 1f), new Color(0.59f, 0.72f, 0.44f, 1f), py);
                float gardenBand = Mathf.SmoothStep(0.30f, 0.58f, py);
                Color c = Color.Lerp(garden, sky, gardenBand);
                float vignette = Vector2.Distance(new Vector2(px, py), new Vector2(0.5f, 0.52f));
                return Color.Lerp(c, new Color(0.18f, 0.30f, 0.34f, 1f), Mathf.Clamp01((vignette - 0.45f) * 0.38f));
            });

            return Store(key, texture, 16f);
        }

        private static Sprite Store(string key, Texture2D texture, float pixelsPerUnit = 100f)
        {
            texture.name = key;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.Apply(false, true);

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        private static Texture2D CreateTexture(int width, int height, System.Func<int, int, Color> pixel)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[y * width + x] = pixel(x, y);
                }
            }

            texture.SetPixels(colors);
            return texture;
        }

        private static Color Clear()
        {
            return new Color(0f, 0f, 0f, 0f);
        }

        private static float RoundedRectDistance(float px, float py, float cx, float cy, float halfW, float halfH, float radius)
        {
            float qx = Mathf.Abs(px - cx) - (halfW - radius);
            float qy = Mathf.Abs(py - cy) - (halfH - radius);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            return outside + inside - radius;
        }

        private static float SymbolMask(int type, float x, float y)
        {
            switch (type)
            {
                case 0: return Soft(LeafMask(x, y));
                case 1: return Soft(DropMask(x, y));
                case 2: return Soft(SunMask(x, y));
                case 3: return Soft(FlowerMask(x, y));
                case 4: return Soft(CrystalMask(x, y));
                default: return Soft(BerryMask(x, y));
            }
        }

        private static bool IsSymbolLine(int type, float x, float y)
        {
            if (type == 0) return Mathf.Abs(x * 0.65f + y * 0.26f) < 0.035f && x > -0.42f && x < 0.38f;
            if (type == 1) return y < -0.03f && Mathf.Abs(x) < 0.035f;
            if (type == 4) return Mathf.Abs(x) < 0.035f && Mathf.Abs(y) < 0.46f;
            return false;
        }

        private static float Soft(float signed)
        {
            return Mathf.Clamp01(0.5f - signed / 0.045f);
        }

        private static float LeafMask(float x, float y)
        {
            float rx = x * 0.92f + y * 0.42f;
            float ry = -x * 0.42f + y * 0.92f;
            return (rx * rx) / 0.34f + (ry * ry) / 0.13f - 1f;
        }

        private static float DropMask(float x, float y)
        {
            float circle = (x * x) / 0.22f + ((y + 0.18f) * (y + 0.18f)) / 0.25f - 1f;
            float top = Mathf.Abs(x) / Mathf.Lerp(0.08f, 0.32f, Mathf.Clamp01(0.74f - y)) + Mathf.Abs(y - 0.38f) / 0.60f - 1f;
            return Mathf.Min(circle, top);
        }

        private static float SunMask(float x, float y)
        {
            float angle = Mathf.Atan2(y, x);
            float ray = Mathf.Abs(Mathf.Sin(angle * 6f)) < 0.20f && x * x + y * y < 0.62f ? -0.12f : 1f;
            float center = Mathf.Sqrt(x * x + y * y) - 0.35f;
            return Mathf.Min(center, ray);
        }

        private static float FlowerMask(float x, float y)
        {
            float mask = Mathf.Sqrt(x * x + y * y) - 0.16f;
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                float cx = Mathf.Cos(a) * 0.28f;
                float cy = Mathf.Sin(a) * 0.28f;
                float petal = ((x - cx) * (x - cx)) / 0.08f + ((y - cy) * (y - cy)) / 0.15f - 1f;
                mask = Mathf.Min(mask, petal);
            }

            return mask;
        }

        private static float CrystalMask(float x, float y)
        {
            return Mathf.Abs(x) / 0.37f + Mathf.Abs(y) / 0.55f - 1f;
        }

        private static float BerryMask(float x, float y)
        {
            float left = ((x + 0.17f) * (x + 0.17f)) / 0.16f + ((y + 0.02f) * (y + 0.02f)) / 0.18f - 1f;
            float right = ((x - 0.19f) * (x - 0.19f)) / 0.15f + ((y + 0.02f) * (y + 0.02f)) / 0.17f - 1f;
            float top = (x * x) / 0.12f + ((y - 0.25f) * (y - 0.25f)) / 0.11f - 1f;
            return Mathf.Min(Mathf.Min(left, right), top);
        }
    }
}
