using UnityEngine;
using BUXPuzzle.Presentation.Audio;

namespace Game.Presentation
{
    public sealed class BoardView : UnityEngine.MonoBehaviour
    {
    [SerializeField]
    private GameObject natureLightTilePrefab;

        public int Width = 8;
        public int Height = 8;
        public float CellSize = 1.1f;
        public GameObject TilePrefab;

        private global::GameRoot _root;
        private Game.Core.BoardEngine _engine;
        private TileView[,] _views;
        private SpriteRenderer _boardSurface;

        [Header("Debug")]
        public bool AutoDrawOnStart = true;
        public int RandomSeed = 12345;

        void Start()
        {
            if (!AutoDrawOnStart) return;

            _root = UnityEngine.Object.FindFirstObjectByType<global::GameRoot>();
            if (_root != null)
            {
                Bind(_root);
            }

            DrawOrRedrawFromEngine();
        }

        public void Bind(global::GameRoot root)
        {
            _root = root;
            _engine = root != null ? root.Board : null;
        }

        public bool RequestSwap(TileView a, TileView b)
        {
            Game.Core.BoardEngine.ResolveSummary ignored;
            return RequestSwap(a, b, out ignored);
        }

        public bool RequestSwap(TileView a, TileView b, out Game.Core.BoardEngine.ResolveSummary summary)
        {
            summary = Game.Core.BoardEngine.ResolveSummary.New();

            if (a == null || b == null)
            {
                Debug.LogError("[BoardView] RequestSwap received null tile reference");
                return false;
            }

            if (_engine == null)
            {
                _root = UnityEngine.Object.FindFirstObjectByType<global::GameRoot>();
                _engine = _root != null ? _root.Board : null;
            }

            if (_engine == null)
            {
                Debug.LogError("[BoardView] No engine bound; cannot swap");
                return false;
            }

            Game.Logging.UlfUnityLogger.Info(
                "game.swap.request",
                "Swap requested",
                $"a={a.X},{a.Y} b={b.X},{b.Y}"
            );

            bool ok = _engine.TrySwapAndResolve(a.X, a.Y, b.X, b.Y, out summary);

            if (ok)
            {
                DrawOrRedrawFromEngine();
                var audio = FBL_PresentationAudioRouter.Ensure();
                audio.PlayEvent("swap");
                audio.PlayEvent(summary.iterations > 1 ? "cascade" : "match");
                audio.PlayEvent("settle");

                var hud = FindFirstObjectByType<Game.UI.SimpleHud>();
                if (hud != null)
                {
                    hud.OnMatch();
                }

                Game.Logging.UlfUnityLogger.Info(
                    "game.swap.accept",
                    "Swap accepted",
                    $"a={a.X},{a.Y} b={b.X},{b.Y} clearedTiles={summary.clearedTiles} iterations={summary.iterations}"
                );

                return true;
            }

            Game.Logging.UlfUnityLogger.Warn(
                "game.swap.reject",
                "Swap rejected",
                $"a={a.X},{a.Y} b={b.X},{b.Y} clearedTiles={summary.clearedTiles} iterations={summary.iterations}"
            );

            FBL_PresentationAudioRouter.Ensure().PlayEvent("invalid_swap");
            StartCoroutine(InvalidSwapRoutine(a, b));
            return false;
        }

        private System.Collections.IEnumerator InvalidSwapRoutine(TileView a, TileView b)
        {
            float t = 0f;
            float dur = 0.18f;
            Vector3 a0 = a.transform.localPosition;
            Vector3 b0 = b.transform.localPosition;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Sin(t * 60f) * 0.08f;
                a.transform.localPosition = a0 + new Vector3(k, 0f, 0f);
                b.transform.localPosition = b0 + new Vector3(-k, 0f, 0f);
                yield return null;
            }

            a.transform.localPosition = a0;
            b.transform.localPosition = b0;

            Game.UI.ToastUI.Show("Ingen match", 1.2f);
        }

        public void DrawOrRedrawFromEngine()
        {
            if (_engine == null)
            {
                _root = UnityEngine.Object.FindFirstObjectByType<global::GameRoot>();
                _engine = _root != null ? _root.Board : null;
            }

            if (_engine == null)
            {
                DrawOrRedraw();
                return;
            }

            Width = _engine.Width;
            Height = _engine.Height;

            var activePrefab = natureLightTilePrefab != null ? natureLightTilePrefab : TilePrefab;
            if (activePrefab == null)
            {
                Debug.LogError("BoardView: no tile prefab assigned");
                return;
            }

            if (_views == null || _views.GetLength(0) != Width || _views.GetLength(1) != Height)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    var child = transform.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
                }

                _views = new TileView[Width, Height];

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var pos = new Vector3(x * CellSize, y * CellSize, 0f);
                        GameObject go = Instantiate(activePrefab, transform);
                        go.name = "Tile_" + x + "_" + y;
                        go.transform.localPosition = pos;

                        var view = go.GetComponent<TileView>();
                        if (view == null) view = go.AddComponent<TileView>();

                        var tile = _engine.Get(x, y);
                        view.Init(x, y, (int)tile.Type);
                        _views[x, y] = view;
                    }
                }

                var offset = new Vector3((Width - 1) * CellSize * 0.5f, (Height - 1) * CellSize * 0.5f, 0f);
                transform.position = -offset;
            }

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var tile = _engine.Get(x, y);
                    int type = (int)tile.Type;

                    _views[x, y].SetCoords(x, y);
                    _views[x, y].SetType(type);
                    _views[x, y].transform.localPosition = new Vector3(x * CellSize, y * CellSize, 0f);
                }
            }

            ApplyScenePresentation();
        }

        public void DrawOrRedraw()
        {
            if (TilePrefab == null)
            {
                Debug.LogError("BoardView: TilePrefab missing");
                return;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }

            var rng = new System.Random(RandomSeed);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var pos = new Vector3(x * CellSize, y * CellSize, 0f);
                    var go = Instantiate(TilePrefab, pos, Quaternion.identity, transform);
                    go.name = "Tile_" + x + "_" + y;

                    var view = go.GetComponent<TileView>();
                    if (view == null) view = go.AddComponent<TileView>();

                    int type = rng.Next(0, 6);
                    view.Init(x, y, type);
                }
            }

            var offset = new Vector3((Width - 1) * CellSize * 0.5f, (Height - 1) * CellSize * 0.5f, 0f);
            transform.position = -offset;
            ApplyScenePresentation();
        }

        private void ApplyScenePresentation()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.51f, 0.72f, 0.78f, 1f);
                cam.orthographicSize = Mathf.Max(5.15f, Height * CellSize * 0.66f);
                cam.transform.position = new Vector3(0f, 0.20f, -10f);
                cam.transform.rotation = Quaternion.identity;
            }

            EnsureBackdrop();
            EnsureBoardSurface();
        }

        private void EnsureBackdrop()
        {
            var backdrop = GameObject.Find("NatureLightBackdrop");
            if (backdrop == null)
            {
                backdrop = new GameObject("NatureLightBackdrop");
            }

            backdrop.transform.position = new Vector3(0f, 0f, 2f);
            backdrop.transform.localRotation = Quaternion.identity;

            var renderer = backdrop.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = backdrop.AddComponent<SpriteRenderer>();
            renderer.sprite = NatureLightRuntimeArt.Backdrop();
            renderer.sortingOrder = -100;
            renderer.color = Color.white;

            if (renderer.sprite != null)
            {
                var bounds = renderer.sprite.bounds.size;
                renderer.transform.localScale = new Vector3(18f / bounds.x, 13f / bounds.y, 1f);
            }
        }

        private void EnsureBoardSurface()
        {
            var child = transform.Find("BoardSurface");
            if (child == null)
            {
                var go = new GameObject("BoardSurface");
                child = go.transform;
                child.SetParent(transform, false);
            }

            child.SetAsFirstSibling();
            child.localRotation = Quaternion.identity;
            child.localPosition = new Vector3((Width - 1) * CellSize * 0.5f, (Height - 1) * CellSize * 0.5f, 0.08f);

            _boardSurface = child.GetComponent<SpriteRenderer>();
            if (_boardSurface == null) _boardSurface = child.gameObject.AddComponent<SpriteRenderer>();
            _boardSurface.sprite = NatureLightRuntimeArt.BoardPanel();
            _boardSurface.sortingOrder = -20;
            _boardSurface.color = Color.white;

            if (_boardSurface.sprite != null)
            {
                var bounds = _boardSurface.sprite.bounds.size;
                float width = Width * CellSize + 0.84f;
                float height = Height * CellSize + 0.84f;
                child.localScale = new Vector3(width / bounds.x, height / bounds.y, 1f);
            }
        }
    }
}

