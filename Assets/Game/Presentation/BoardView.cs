using System;
using System.Collections.Generic;
using Game.Audio;
using Game.Content;
using Game.Core;
using Game.Presentation.Juice;
using Game.Presentation.Layout;
using Game.Telemetry;
using Game.UI;
using UnityEngine;
using ResolveSummary = Game.Core.BoardEngine.ResolveSummary;

namespace Game.Presentation
{
    public sealed class BoardView : UnityEngine.MonoBehaviour
    {
        [SerializeField]
        private GameObject natureLightTilePrefab;
        [SerializeField]
        private TileSetConfig tileSetConfig;
        [SerializeField]
        [Range(0.72f, 1.18f)]
        private float tileVisualScale = 1f;

        public int Width = 8;
        public int Height = 8;
        public float CellSize = 1.1f;
        public GameObject TilePrefab;

        private global::GameRoot _root;
        private BoardEngine _engine;
        private TileView[,] _views;
        private SpriteRenderer _boardSurface;
        private Transform _slotRoot;
        private string _slotLayoutKey;
        private BoardResolveAnimator _resolveAnimator;
        private bool _isResolving;
        private int _invalidSwapCount;
        private bool _firstMatchSent;
        private bool _boardVisibleSent;
        private BoardMove _tutorialMove;
        private bool _hasTutorialMove;
        private bool _tutorialStrongHint;

        public bool IsResolving => _isResolving;
        public bool TutorialActive => _hasTutorialMove;
        public event Action<BoardView> BoardVisible;
        public event Action<BoardMove, ResolveSummary, ResolveTrace, string> SwapAccepted;
        public event Action<BoardMove, string> SwapRejected;

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

        public void ResetBoardVisibleTelemetry()
        {
            _boardVisibleSent = false;
        }

        public bool RequestSwap(TileView a, TileView b)
        {
            ResolveSummary ignored;
            return RequestSwap(a, b, out ignored);
        }

        public bool RequestSwap(TileView a, TileView b, out ResolveSummary summary)
        {
            return RequestSwap(a, b, out summary, "tap");
        }

        public bool RequestSwap(TileView a, TileView b, out ResolveSummary summary, string inputType)
        {
            summary = ResolveSummary.New();

            if (a == null || b == null)
            {
                Debug.LogError("[BoardView] RequestSwap received null tile reference");
                return false;
            }

            if (_isResolving)
            {
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

            var move = new BoardMove(new BoardCoord(a.X, a.Y), new BoardCoord(b.X, b.Y));
            GameTelemetry.Track("game.swap.request", GameTelemetry.Props(
                "input_type", inputType,
                "tutorial_active", TutorialActive,
                "from", move.A.ToString(),
                "to", move.B.ToString()
            ));

            Game.Logging.UlfUnityLogger.Info(
                "game.swap.request",
                "Swap requested",
                $"a={a.X},{a.Y} b={b.X},{b.Y}"
            );

            ResolveTrace trace;
            bool ok = _engine.TrySwapAndResolveWithTrace(a.X, a.Y, b.X, b.Y, out trace);
            summary = trace != null ? trace.Summary : ResolveSummary.New();

            if (ok)
            {
                var hud = FindFirstObjectByType<Game.UI.SimpleHud>();
                if (hud != null)
                {
                    hud.OnMatch(summary.clearedTiles, summary.iterations);
                }

                GameTelemetry.Track("game.swap.accept", GameTelemetry.Props(
                    "input_type", inputType,
                    "tutorial_active", TutorialActive,
                    "cleared_tile_count", summary.clearedTiles,
                    "cascade_iteration_count", summary.iterations
                ));

                if (!_firstMatchSent)
                {
                    _firstMatchSent = true;
                    GameTelemetry.Track("game.first_match", GameTelemetry.Props(
                        "cleared_tile_count", summary.clearedTiles,
                        "cascade_iteration_count", summary.iterations
                    ));
                }

                if (trace != null)
                {
                    for (int i = 0; i < trace.Steps.Count; i++)
                    {
                        var step = trace.Steps[i];
                        GameTelemetry.Track("game.resolve.iteration", GameTelemetry.Props(
                            "iteration", step.Iteration,
                            "cleared_tile_count", step.Cleared.Count,
                            "drop_count", step.Drops.Count,
                            "spawn_count", step.Spawned.Count
                        ));
                    }

                    if (trace.Steps.Count > 1)
                    {
                        GameTelemetry.Track("game.cascade", GameTelemetry.Props("cascade_iteration_count", trace.Steps.Count));
                    }
                }

                Game.Logging.UlfUnityLogger.Info(
                    "game.swap.accept",
                    "Swap accepted",
                    $"a={a.X},{a.Y} b={b.X},{b.Y} clearedTiles={summary.clearedTiles} iterations={summary.iterations}"
                );

                _isResolving = true;
                StartCoroutine(ResolvePresentationRoutine(a, b, move, summary, trace, inputType));
                return true;
            }

            GameTelemetry.Track("game.swap.reject", GameTelemetry.Props(
                "input_type", inputType,
                "tutorial_active", TutorialActive,
                "from", move.A.ToString(),
                "to", move.B.ToString()
            ));

            Game.Logging.UlfUnityLogger.Warn(
                "game.swap.reject",
                "Swap rejected",
                $"a={a.X},{a.Y} b={b.X},{b.Y} clearedTiles={summary.clearedTiles} iterations={summary.iterations}"
            );

            BoardJuiceController.Ensure().SwapRejected();
            SwapRejected?.Invoke(move, inputType);
            StartCoroutine(InvalidSwapRoutine(a, b));
            return false;
        }

        private System.Collections.IEnumerator ResolvePresentationRoutine(TileView a, TileView b, BoardMove move, ResolveSummary summary, ResolveTrace trace, string inputType)
        {
            if (_resolveAnimator == null)
            {
                _resolveAnimator = GetComponent<BoardResolveAnimator>();
                if (_resolveAnimator == null) _resolveAnimator = gameObject.AddComponent<BoardResolveAnimator>();
            }

            yield return _resolveAnimator.Play(this, a, b, trace);
            DrawOrRedrawFromEngine();
            GameAudioController.Ensure().Play(AudioEvent.DropLand, 0.55f);
            _isResolving = false;
            SwapAccepted?.Invoke(move, summary, trace, inputType);
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

            _invalidSwapCount++;
            string copy = TutorialActive ? UXCopy.TutorialInvalid : (_invalidSwapCount == 1 ? UXCopy.FirstInvalidSwap : UXCopy.LaterInvalidSwap);
            ToastUI.Show(copy, _invalidSwapCount == 1 || TutorialActive ? 2.2f : 1.3f);
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

            if (_views == null || _views.GetLength(0) != Width || _views.GetLength(1) != Height || HasMissingTileViews())
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
                        if (!_engine.IsCellActive(x, y))
                        {
                            _views[x, y] = null;
                            continue;
                        }

                        var pos = new Vector3(x * CellSize, y * CellSize, 0f);
                        GameObject go = Instantiate(activePrefab, transform);
                        go.name = "Tile_" + x + "_" + y;
                        go.transform.localPosition = pos;

                        var view = go.GetComponent<TileView>();
                        if (view == null) view = go.AddComponent<TileView>();
                        view.SetTileSet(tileSetConfig);
                        view.SetVisualScale(tileVisualScale);

                        var tile = _engine.Get(x, y);
                        view.Init(x, y, (int)tile.Type);
                        view.SetState(tile.State);
                        view.SetSpecial(tile.Special);
                        view.SetBlocker(_engine.GetCell(x, y).Blocker);
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
                    if (!_engine.IsCellActive(x, y))
                    {
                        continue;
                    }

                    var tile = _engine.Get(x, y);
                    int type = (int)tile.Type;

                    if (_views[x, y] == null) continue;
                    _views[x, y].SetTileSet(tileSetConfig);
                    _views[x, y].SetVisualScale(tileVisualScale);
                    _views[x, y].SetCoords(x, y);
                    _views[x, y].SetType(type);
                    _views[x, y].SetState(tile.State);
                    _views[x, y].SetSpecial(tile.Special);
                    _views[x, y].SetBlocker(_engine.GetCell(x, y).Blocker);
                    _views[x, y].transform.localPosition = new Vector3(x * CellSize, y * CellSize, 0f);
                }
            }

            ApplyTutorialHighlight();
            ApplyScenePresentation();
            SendBoardVisibleOnce();
        }

        private bool HasMissingTileViews()
        {
            if (_views == null) return true;

            for (int y = 0; y < _views.GetLength(1); y++)
            {
                for (int x = 0; x < _views.GetLength(0); x++)
                {
                    if (_engine != null && !_engine.IsCellActive(x, y))
                    {
                        continue;
                    }

                    if (_views[x, y] == null)
                    {
                        return true;
                    }
                }
            }

            return false;
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
                    view.SetTileSet(tileSetConfig);
                    view.SetVisualScale(tileVisualScale);

                    int type = rng.Next(0, 6);
                    view.Init(x, y, type);
                }
            }

            var offset = new Vector3((Width - 1) * CellSize * 0.5f, (Height - 1) * CellSize * 0.5f, 0f);
            transform.position = -offset;
            ApplyTutorialHighlight();
            ApplyScenePresentation();
            SendBoardVisibleOnce();
        }

        public TileView GetTileView(int x, int y)
        {
            if (_views == null) return null;
            if (x < 0 || x >= _views.GetLength(0) || y < 0 || y >= _views.GetLength(1)) return null;
            return _views[x, y];
        }

        public Vector3 LocalPositionFor(int x, int y)
        {
            return new Vector3(x * CellSize, y * CellSize, 0f);
        }

        public void SetTutorialMove(BoardMove move, bool strongHint)
        {
            _tutorialMove = move;
            _hasTutorialMove = true;
            _tutorialStrongHint = strongHint;
            ApplyTutorialHighlight();
        }

        public void ClearTutorialMove()
        {
            _hasTutorialMove = false;
            ApplyTutorialHighlight();
        }

        public bool IsTutorialMove(BoardMove move)
        {
            if (!_hasTutorialMove) return false;
            return SameMove(_tutorialMove, move);
        }

        public void SwapVisualSlots(TileView a, TileView b)
        {
            if (_views == null || a == null || b == null) return;

            int ax = a.X;
            int ay = a.Y;
            int bx = b.X;
            int by = b.Y;

            _views[ax, ay] = b;
            _views[bx, by] = a;
            a.SetCoords(bx, by);
            b.SetCoords(ax, ay);
        }

        public void SetTilesResolveHighlight(List<BoardCoord> coords, bool active)
        {
            if (coords == null) return;
            foreach (var coord in coords)
            {
                var tile = GetTileView(coord.X, coord.Y);
                if (tile != null) tile.SetResolveHighlight(active);
            }
        }

        public void SetTilesClearing(List<BoardCoord> coords, bool active)
        {
            if (coords == null) return;
            foreach (var coord in coords)
            {
                var tile = GetTileView(coord.X, coord.Y);
                if (tile != null) tile.SetClearing(active);
            }
        }

        public void PulseSpawnedTiles(List<SpawnedTile> spawned)
        {
            if (spawned == null) return;
            foreach (var spawn in spawned)
            {
                var tile = GetTileView(spawn.Coord.X, spawn.Coord.Y);
                if (tile != null) tile.SetResolveHighlight(true);
            }

            StartCoroutine(ClearSpawnPulse(spawned));
        }

        public void ApplyDropVisualSlots(List<DropMove> drops)
        {
            if (_views == null || drops == null || drops.Count == 0) return;

            var next = (TileView[,])_views.Clone();
            foreach (var drop in drops)
            {
                if (!IsInViewBounds(drop.From.X, drop.From.Y)) continue;
                next[drop.From.X, drop.From.Y] = null;
            }

            foreach (var drop in drops)
            {
                if (!IsInViewBounds(drop.From.X, drop.From.Y) || !IsInViewBounds(drop.To.X, drop.To.Y)) continue;
                var tile = _views[drop.From.X, drop.From.Y];
                if (tile == null) continue;
                next[drop.To.X, drop.To.Y] = tile;
                tile.SetCoords(drop.To.X, drop.To.Y);
            }

            _views = next;
        }

        private System.Collections.IEnumerator ClearSpawnPulse(List<SpawnedTile> spawned)
        {
            yield return new WaitForSeconds(0.14f);
            foreach (var spawn in spawned)
            {
                var tile = GetTileView(spawn.Coord.X, spawn.Coord.Y);
                if (tile != null) tile.SetResolveHighlight(false);
            }
        }

        private void ApplyTutorialHighlight()
        {
            if (_views == null) return;

            for (int y = 0; y < _views.GetLength(1); y++)
            {
                for (int x = 0; x < _views.GetLength(0); x++)
                {
                    var tile = _views[x, y];
                    if (tile != null) tile.SetTutorialHint(false, false);
                }
            }

            if (!_hasTutorialMove) return;

            var a = GetTileView(_tutorialMove.A.X, _tutorialMove.A.Y);
            var b = GetTileView(_tutorialMove.B.X, _tutorialMove.B.Y);
            if (a != null) a.SetTutorialHint(true, _tutorialStrongHint);
            if (b != null) b.SetTutorialHint(true, _tutorialStrongHint);
        }

        private static bool SameMove(BoardMove a, BoardMove b)
        {
            return (a.A.X == b.A.X && a.A.Y == b.A.Y && a.B.X == b.B.X && a.B.Y == b.B.Y) ||
                   (a.A.X == b.B.X && a.A.Y == b.B.Y && a.B.X == b.A.X && a.B.Y == b.A.Y);
        }

        private void SendBoardVisibleOnce()
        {
            if (_boardVisibleSent || _engine == null) return;
            _boardVisibleSent = true;
            GameTelemetry.Track("session.board_visible", GameTelemetry.Props(
                "board_width", _engine.Width,
                "board_height", _engine.Height
            ));
            BoardVisible?.Invoke(this);
        }

        private bool IsInViewBounds(int x, int y)
        {
            return _views != null && x >= 0 && x < _views.GetLength(0) && y >= 0 && y < _views.GetLength(1);
        }

        private void ApplyScenePresentation()
        {
            var cam = Camera.main;
            BoardLayoutController.Apply(cam, transform, Width, Height, CellSize);

            EnsureBackdrop();
            EnsureBoardSurface();
            EnsureCellSlots();
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
                var cam = Camera.main;
                float height = cam != null ? cam.orthographicSize * 2.3f : 13f;
                float width = cam != null ? height * Mathf.Max(1f, cam.aspect) : 18f;
                renderer.transform.localScale = new Vector3(width / bounds.x, height / bounds.y, 1f);
            }
        }

        private void EnsureBoardSurface()
        {
            var shadow = transform.Find("BoardShadow");
            if (shadow == null)
            {
                var shadowGo = new GameObject("BoardShadow");
                shadow = shadowGo.transform;
                shadow.SetParent(transform, false);
            }

            shadow.SetAsFirstSibling();
            shadow.localRotation = Quaternion.identity;
            shadow.localPosition = new Vector3((Width - 1) * CellSize * 0.5f + 0.08f, (Height - 1) * CellSize * 0.5f - 0.10f, 0.09f);
            var shadowRenderer = shadow.GetComponent<SpriteRenderer>();
            if (shadowRenderer == null) shadowRenderer = shadow.gameObject.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = NatureLightRuntimeArt.BoardPanel();
            shadowRenderer.sortingOrder = -30;
            shadowRenderer.color = new Color(0.04f, 0.12f, 0.12f, 0.34f);

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
                shadow.localScale = new Vector3((width + 0.16f) / bounds.x, (height + 0.16f) / bounds.y, 1f);
            }
        }

        private void EnsureCellSlots()
        {
            string key = BuildSlotLayoutKey();
            if (_slotRoot != null && _slotLayoutKey == key)
            {
                return;
            }

            var root = transform.Find("CellSlots");
            if (root == null)
            {
                var go = new GameObject("CellSlots");
                root = go.transform;
                root.SetParent(transform, false);
            }

            _slotRoot = root;
            _slotLayoutKey = key;
            _slotRoot.localPosition = Vector3.zero;
            _slotRoot.localRotation = Quaternion.identity;

            for (int i = _slotRoot.childCount - 1; i >= 0; i--)
            {
                var child = _slotRoot.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }

            var slotSprite = tileSetConfig != null && tileSetConfig.CellSlotSprite != null ? tileSetConfig.CellSlotSprite : NatureLightRuntimeArt.CellSlot();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!CellActiveForPresentation(x, y)) continue;

                    var go = new GameObject("CellSlot_" + x + "_" + y);
                    var child = go.transform;
                    child.SetParent(_slotRoot, false);
                    child.localPosition = new Vector3(x * CellSize, y * CellSize, 0.06f);
                    child.localRotation = Quaternion.identity;

                    var renderer = go.AddComponent<SpriteRenderer>();
                    renderer.sprite = slotSprite;
                    renderer.sortingOrder = -12;
                    renderer.color = Color.white;
                    ScaleRendererToCell(renderer, CellSize * 0.92f);
                }
            }
        }

        private string BuildSlotLayoutKey()
        {
            var builder = new System.Text.StringBuilder(Width * Height + 16);
            builder.Append(Width).Append('x').Append(Height).Append('@').Append(CellSize.ToString("0.###")).Append(':');
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    builder.Append(CellActiveForPresentation(x, y) ? '1' : '0');
                }
            }

            return builder.ToString();
        }

        private bool CellActiveForPresentation(int x, int y)
        {
            return _engine == null || _engine.IsCellActive(x, y);
        }

        private static void ScaleRendererToCell(SpriteRenderer renderer, float targetSize)
        {
            if (renderer == null || renderer.sprite == null) return;

            var size = renderer.sprite.bounds.size;
            float largest = Mathf.Max(size.x, size.y);
            if (largest <= 0.0001f) return;

            float scale = targetSize / largest;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

