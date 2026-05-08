using System;
using System.Collections.Generic;

namespace Game.Core
{
    public sealed class BoardEngine
    {
        public struct ResolveSummary
        {
            public static ResolveSummary New()
            {
                var s = new ResolveSummary();
                s.iterations = 0;
                s.clearedTiles = 0;
                return s;
            }

            public int iterations;
            public int clearedTiles;
            public bool anyCleared => clearedTiles > 0;
        }

        public readonly int Width;
        public readonly int Height;

        private readonly Tile[] _tiles;
        private readonly System.Random _rng;

        public BoardEngine(int w, int h, int seed)
        {
            Width = w;
            Height = h;
            _tiles = new Tile[w * h];
            _rng = new System.Random(seed);

            FillRandom();
            EnsureStableStart();
        }

        public Tile Get(int x, int y) => _tiles[y * Width + x];
        public void Set(int x, int y, Tile t) => _tiles[y * Width + x] = t;

        public void Swap(int x1, int y1, int x2, int y2)
        {
            var a = Get(x1, y1);
            var b = Get(x2, y2);
            Set(x1, y1, b);
            Set(x2, y2, a);
        }

        public bool Resolve()
        {
            ResolveSummary s;
            Resolve(out s, 50);
            return s.anyCleared;
        }

        public void EnsureStableStart(int maxAttempts = 200)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (FindMatches().Count == 0 && HasAnyValidMove())
                {
                    return;
                }

                FillRandom();
            }

            throw new InvalidOperationException("BoardEngine.EnsureStableStart failed to generate a stable board with at least one valid move.");
        }

        public bool HasAnyValidMove()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x + 1 < Width && WouldSwapCreateMatch(x, y, x + 1, y)) return true;
                    if (y + 1 < Height && WouldSwapCreateMatch(x, y, x, y + 1)) return true;
                }
            }

            return false;
        }

        private bool WouldSwapCreateMatch(int x1, int y1, int x2, int y2)
        {
            Swap(x1, y1, x2, y2);
            bool ok = FindMatches().Count > 0;
            Swap(x1, y1, x2, y2);
            return ok;
        }

        public bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            return (Math.Abs(x1 - x2) + Math.Abs(y1 - y2)) == 1;
        }

        public bool TrySwapAndResolve(int x1, int y1, int x2, int y2, out ResolveSummary summary, int maxIterations = 50)
        {
            summary = ResolveSummary.New();

            if (!IsAdjacent(x1, y1, x2, y2))
            {
                return false;
            }

            Swap(x1, y1, x2, y2);

            Resolve(out summary, maxIterations);

            if (!summary.anyCleared)
            {
                Swap(x1, y1, x2, y2);
                return false;
            }

            return true;
        }

        public void Resolve(out ResolveSummary summary, int maxIterations = 50)
        {
            summary = ResolveSummary.New();
            int guard = 0;

            while (true)
            {
                var matches = FindMatches();
                if (matches.Count == 0)
                {
                    break;
                }

                Clear(matches, ref summary.clearedTiles);
                Drop();
                Spawn();

                summary.iterations++;
                guard++;

                if (guard >= maxIterations)
                {
                    break;
                }
            }
        }

        private void FillRandom()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                _tiles[i] = RandomTile();
            }
        }

        private Tile RandomTile()
        {
            return new Tile
            {
                Type = (TileType)_rng.Next(0, 6),
                State = TileState.Normal,
                Fx = 0
            };
        }

        private List<int> FindMatches()
        {
            var result = new HashSet<int>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width - 2; x++)
                {
                    int i = y * Width + x;
                    var t = _tiles[i];
                    if (t.State != TileState.Normal) continue;

                    if (_tiles[i + 1].Type == t.Type && _tiles[i + 2].Type == t.Type)
                    {
                        result.Add(i);
                        result.Add(i + 1);
                        result.Add(i + 2);
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height - 2; y++)
                {
                    int i = y * Width + x;
                    var t = _tiles[i];
                    if (t.State != TileState.Normal) continue;

                    if (_tiles[i + Width].Type == t.Type && _tiles[i + Width * 2].Type == t.Type)
                    {
                        result.Add(i);
                        result.Add(i + Width);
                        result.Add(i + Width * 2);
                    }
                }
            }

            return new List<int>(result);
        }

        private void Clear(List<int> indices, ref int clearedTiles)
        {
            foreach (var i in indices)
            {
                if (_tiles[i].State == TileState.Normal)
                {
                    clearedTiles++;
                }

                _tiles[i].State = TileState.Blocker;
            }
        }

        private void Clear(List<int> indices)
        {
            int dummy = 0;
            Clear(indices, ref dummy);
        }

        private void Drop()
        {
            for (int x = 0; x < Width; x++)
            {
                int writeY = 0;

                for (int y = 0; y < Height; y++)
                {
                    var t = Get(x, y);
                    if (t.State != TileState.Blocker)
                    {
                        Set(x, writeY, t);
                        writeY++;
                    }
                }

                for (int y = writeY; y < Height; y++)
                {
                    Set(x, y, new Tile { State = TileState.Blocker });
                }
            }
        }

        private void Spawn()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].State == TileState.Blocker)
                {
                    _tiles[i] = RandomTile();
                }
            }
        }
    }
}
