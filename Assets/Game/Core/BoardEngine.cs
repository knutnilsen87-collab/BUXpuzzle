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
        private readonly BoardCell[] _cells;
        private readonly bool[] _initialDropObjects;
        private readonly System.Random _rng;

        public BoardEngine(int w, int h, int seed)
            : this(w, h, seed, null)
        {
        }

        public BoardEngine(int w, int h, int seed, string[] boardRows)
        {
            Width = w;
            Height = h;
            _tiles = new Tile[w * h];
            _cells = new BoardCell[w * h];
            _initialDropObjects = new bool[w * h];
            _rng = new System.Random(seed);

            InitializeCells(boardRows);
            FillRandom();
            EnsureStableStart();
        }

        public Tile Get(int x, int y) => _tiles[y * Width + x];
        public void Set(int x, int y, Tile t) => _tiles[y * Width + x] = t;
        public BoardCell GetCell(int x, int y) => _cells[y * Width + x];
        public bool IsCellActive(int x, int y) => IsInBounds(x, y) && _cells[y * Width + x].Active;

        public void Swap(int x1, int y1, int x2, int y2)
        {
            var a = Get(x1, y1);
            var b = Get(x2, y2);
            Set(x1, y1, b);
            Set(x2, y2, a);
        }

        public void SeedBlockers(TileState state, int count)
        {
            if (state != TileState.Frozen && state != TileState.Locked) return;
            int placed = 0;
            int attempts = 0;
            while (placed < count && attempts < Width * Height * 4)
            {
                attempts++;
                int x = _rng.Next(0, Width);
                int y = _rng.Next(0, Height);
                int index = y * Width + x;
                if (!CellCanHoldTile(index) || _cells[index].Blocker != CellBlockerType.None) continue;
                var tile = Get(x, y);
                if (tile.State != TileState.Normal) continue;
                var cell = _cells[index];
                cell.Blocker = state == TileState.Frozen ? CellBlockerType.Moss : CellBlockerType.Vine;
                cell.BlockerHp = 1;
                _cells[index] = cell;
                tile.State = state;
                Set(x, y, tile);
                placed++;
            }
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

        public bool WouldSwapCreateMatch(int x1, int y1, int x2, int y2)
        {
            if (!IsInBounds(x1, y1) || !IsInBounds(x2, y2)) return false;
            if (!IsAdjacent(x1, y1, x2, y2)) return false;
            if (!IsSwappable(x1, y1) || !IsSwappable(x2, y2)) return false;

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
            ResolveTrace trace;
            bool ok = TrySwapAndResolveWithTrace(x1, y1, x2, y2, out trace, maxIterations);
            summary = trace != null ? trace.Summary : ResolveSummary.New();
            return ok;
        }

        public bool TrySwapAndResolveWithTrace(int x1, int y1, int x2, int y2, out ResolveTrace trace, int maxIterations = 50)
        {
            trace = new ResolveTrace
            {
                Swap = new BoardMove(new BoardCoord(x1, y1), new BoardCoord(x2, y2)),
                Summary = ResolveSummary.New()
            };

            if (!IsInBounds(x1, y1) || !IsInBounds(x2, y2) || !IsAdjacent(x1, y1, x2, y2))
            {
                return false;
            }

            if (!IsSwappable(x1, y1) || !IsSwappable(x2, y2))
            {
                return false;
            }

            var beforeA = Get(x1, y1);
            var beforeB = Get(x2, y2);
            if (IsSpecial(beforeA.State) || IsSpecial(beforeB.State))
            {
                ActivateSpecialSwap(x1, y1, x2, y2, beforeA, beforeB, trace, maxIterations);
                return trace.Summary.anyCleared;
            }

            Swap(x1, y1, x2, y2);

            Resolve(out var summary, maxIterations, trace);
            trace.Summary = summary;

            if (!summary.anyCleared)
            {
                Swap(x1, y1, x2, y2);
                trace.Summary = ResolveSummary.New();
                trace.Steps.Clear();
                return false;
            }

            CreateSpecialFromLargeMatch(x1, y1, summary.clearedTiles, trace);

            if (!HasAnyValidMove())
            {
                EnsureStableStart();
            }

            return true;
        }

        public void Resolve(out ResolveSummary summary, int maxIterations = 50)
        {
            Resolve(out summary, maxIterations, null);
        }

        private void Resolve(out ResolveSummary summary, int maxIterations, ResolveTrace trace)
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

                ResolveStep step = null;
                if (trace != null)
                {
                    step = new ResolveStep { Iteration = summary.iterations + 1 };
                    foreach (var match in matches)
                    {
                        step.Matched.Add(ToCoord(match));
                        step.Cleared.Add(ToCoord(match));
                    }
                }

                AddAdjacentBlockers(matches, trace);
                Clear(matches, ref summary.clearedTiles);
                Drop(step);
                Spawn(step != null ? step.Spawned : null);
                if (trace != null && step != null && step.DropObjectsCollected.Count > 0)
                {
                    trace.DropObjectsCollected.AddRange(step.DropObjectsCollected);
                }

                summary.iterations++;
                if (step != null)
                {
                    trace.Steps.Add(step);
                }

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
                if (!CellCanHoldTile(i))
                {
                    _tiles[i] = TileForBlockedCell(i);
                    continue;
                }

                _tiles[i] = RandomTile();
                if (_initialDropObjects[i])
                {
                    _tiles[i].Fx = 1;
                }
                ApplyCellBlockerVisual(i);
            }
        }

        private void InitializeCells(string[] boardRows)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new BoardCell
                {
                    Active = true,
                    Blocker = CellBlockerType.None,
                    BlockerHp = 0,
                    IsSpawnPoint = true,
                    IsDropExit = false
                };
            }

            if (boardRows == null || boardRows.Length == 0) return;
            for (int y = 0; y < Height && y < boardRows.Length; y++)
            {
                string row = boardRows[y] ?? string.Empty;
                for (int x = 0; x < Width && x < row.Length; x++)
                {
                    int index = y * Width + x;
                    var cell = _cells[index];
                    switch (row[x])
                    {
                        case '#':
                            cell.Active = false;
                            cell.IsSpawnPoint = false;
                            cell.Blocker = CellBlockerType.None;
                            cell.BlockerHp = 0;
                            break;
                        case 'M':
                            cell.Blocker = CellBlockerType.Moss;
                            cell.BlockerHp = 1;
                            break;
                        case 'V':
                            cell.Blocker = CellBlockerType.Vine;
                            cell.BlockerHp = 1;
                            break;
                        case 'P':
                            cell.Blocker = CellBlockerType.Pebble;
                            cell.BlockerHp = 2;
                            break;
                        case 'I':
                            cell.Blocker = CellBlockerType.Ice;
                            cell.BlockerHp = 1;
                            break;
                        case 'D':
                            cell.IsDropExit = true;
                            break;
                        case 'O':
                            cell.IsSpawnPoint = true;
                            _initialDropObjects[index] = true;
                            break;
                        case 'S':
                            cell.IsSpawnPoint = true;
                            break;
                    }

                    _cells[index] = cell;
                }
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
                    if (!CellCanHoldTile(i) || !CellCanHoldTile(i + 1) || !CellCanHoldTile(i + 2)) continue;
                    var t = _tiles[i];
                    if (!IsMatchable(t.State)) continue;

                    if (IsMatchable(_tiles[i + 1].State) && IsMatchable(_tiles[i + 2].State) &&
                        _tiles[i + 1].Type == t.Type && _tiles[i + 2].Type == t.Type)
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
                    if (!CellCanHoldTile(i) || !CellCanHoldTile(i + Width) || !CellCanHoldTile(i + Width * 2)) continue;
                    var t = _tiles[i];
                    if (!IsMatchable(t.State)) continue;

                    if (IsMatchable(_tiles[i + Width].State) && IsMatchable(_tiles[i + Width * 2].State) &&
                        _tiles[i + Width].Type == t.Type && _tiles[i + Width * 2].Type == t.Type)
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
                if (IsMatchable(_tiles[i].State) || IsBlocker(_tiles[i].State))
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

        private void Drop(ResolveStep step = null)
        {
            for (int x = 0; x < Width; x++)
            {
                int writeY = 0;

                for (int y = 0; y < Height; y++)
                {
                    int readIndex = y * Width + x;
                    if (!CellCanHoldTile(readIndex)) continue;

                    var t = Get(x, y);
                    if (t.State != TileState.Blocker)
                    {
                        while (writeY < Height && !CellCanHoldTile(writeY * Width + x))
                        {
                            writeY++;
                        }

                        if (writeY >= Height) break;
                        Set(x, writeY, t);
                        if (step != null && writeY != y)
                        {
                            step.Drops.Add(new DropMove
                            {
                                From = new BoardCoord(x, y),
                                To = new BoardCoord(x, writeY),
                                Type = (int)t.Type
                            });
                        }

                        writeY++;
                    }
                }

                for (int y = writeY; y < Height; y++)
                {
                    int index = y * Width + x;
                    if (CellCanHoldTile(index))
                    {
                        Set(x, y, new Tile { State = TileState.Blocker });
                    }
                }
            }

            CollectDropObjects(step);
        }

        private void Spawn(List<SpawnedTile> spawned = null)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (CellCanHoldTile(i) && _tiles[i].State == TileState.Blocker)
                {
                    _tiles[i] = RandomTile();
                    if (spawned != null)
                    {
                        spawned.Add(new SpawnedTile
                        {
                            Coord = ToCoord(i),
                            Type = (int)_tiles[i].Type
                        });
                    }
                }
            }
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private static bool IsMatchable(TileState state)
        {
            return state == TileState.Normal || state == TileState.Line || state == TileState.Burst || state == TileState.ColorBomb;
        }

        private static bool IsSpecial(TileState state)
        {
            return state == TileState.Line || state == TileState.Burst || state == TileState.ColorBomb;
        }

        private static bool IsBlocker(TileState state)
        {
            return state == TileState.Frozen || state == TileState.Locked || state == TileState.Pebble || state == TileState.Ice;
        }

        private void AddAdjacentBlockers(List<int> matches, ResolveTrace trace)
        {
            if (matches == null || matches.Count == 0) return;
            var hit = new HashSet<int>();
            foreach (int index in matches)
            {
                int x = index % Width;
                int y = index / Width;
                AddBlockerAt(x + 1, y, hit);
                AddBlockerAt(x - 1, y, hit);
                AddBlockerAt(x, y + 1, hit);
                AddBlockerAt(x, y - 1, hit);
            }

            foreach (int index in hit)
            {
                var cell = _cells[index];
                if (cell.Blocker == CellBlockerType.None) continue;
                var before = cell.Blocker;
                cell.BlockerHp = Math.Max(0, cell.BlockerHp - 1);
                trace?.BlockersHit.Add(new BlockerHitEvent
                {
                    Coord = ToCoord(index),
                    Type = before,
                    HpAfterHit = cell.BlockerHp
                });

                if (cell.BlockerHp <= 0)
                {
                    cell.Blocker = CellBlockerType.None;
                    cell.BlockerHp = 0;
                    _cells[index] = cell;
                    _tiles[index].State = TileState.Blocker;
                    trace?.BlockersCleared.Add(new BlockerClearedEvent
                    {
                        Coord = ToCoord(index),
                        Type = before
                    });
                }
                else
                {
                    _cells[index] = cell;
                }
            }
        }

        private void AddBlockerAt(int x, int y, HashSet<int> add)
        {
            if (!IsInBounds(x, y)) return;
            int index = y * Width + x;
            if (_cells[index].Active && _cells[index].Blocker != CellBlockerType.None)
            {
                add.Add(index);
            }
        }

        private void CreateSpecialFromLargeMatch(int x, int y, int clearedTiles, ResolveTrace trace)
        {
            if (!IsInBounds(x, y) || clearedTiles < 4) return;
            if (!CellCanHoldTile(y * Width + x)) return;

            var tile = Get(x, y);
            if (!IsMatchable(tile.State)) return;

            if (clearedTiles >= 7) tile.State = TileState.ColorBomb;
            else if (clearedTiles >= 5) tile.State = TileState.Burst;
            else tile.State = TileState.Line;

            Set(x, y, tile);
            trace?.SpecialsCreated.Add(new SpecialCreatedEvent
            {
                Coord = new BoardCoord(x, y),
                State = tile.State
            });
        }

        private void ActivateSpecialSwap(int x1, int y1, int x2, int y2, Tile a, Tile b, ResolveTrace trace, int maxIterations)
        {
            var clear = new HashSet<int>();
            AddSpecialClear(x1, y1, a, b.Type, clear);
            AddSpecialClear(x2, y2, b, a.Type, clear);
            if (IsSpecial(a.State)) trace?.SpecialsActivated.Add(new SpecialActivatedEvent { Coord = new BoardCoord(x1, y1), State = a.State });
            if (IsSpecial(b.State)) trace?.SpecialsActivated.Add(new SpecialActivatedEvent { Coord = new BoardCoord(x2, y2), State = b.State });

            if (clear.Count == 0)
            {
                return;
            }

            var step = new ResolveStep { Iteration = 1 };
            foreach (int index in clear)
            {
                step.Matched.Add(ToCoord(index));
                step.Cleared.Add(ToCoord(index));
            }

            int clearedTiles = 0;
            Clear(new List<int>(clear), ref clearedTiles);
            Drop(step);
            Spawn(step.Spawned);
            if (step.DropObjectsCollected.Count > 0)
            {
                trace.DropObjectsCollected.AddRange(step.DropObjectsCollected);
            }
            trace.Steps.Add(step);
            trace.Summary = new ResolveSummary { iterations = 1, clearedTiles = clearedTiles };

            Resolve(out var cascadeSummary, maxIterations, trace);
            trace.Summary = new ResolveSummary
            {
                iterations = trace.Steps.Count,
                clearedTiles = clearedTiles + cascadeSummary.clearedTiles
            };

            if (!HasAnyValidMove())
            {
                EnsureStableStart();
            }
        }

        private void AddSpecialClear(int x, int y, Tile tile, TileType targetType, HashSet<int> clear)
        {
            if (!IsInBounds(x, y) || !IsSpecial(tile.State)) return;

            if (tile.State == TileState.Line)
            {
                for (int cx = 0; cx < Width; cx++) AddClearIfPassable(cx, y, clear);
                for (int cy = 0; cy < Height; cy++) AddClearIfPassable(x, cy, clear);
                return;
            }

            if (tile.State == TileState.Burst)
            {
                for (int cy = y - 1; cy <= y + 1; cy++)
                {
                    for (int cx = x - 1; cx <= x + 1; cx++)
                    {
                        AddClearIfPassable(cx, cy, clear);
                    }
                }
                return;
            }

            if (tile.State == TileState.ColorBomb)
            {
                for (int i = 0; i < _tiles.Length; i++)
                {
                    if (CellCanHoldTile(i) && IsMatchable(_tiles[i].State) && _tiles[i].Type == targetType)
                    {
                        clear.Add(i);
                    }
                }
            }
        }

        private bool IsSwappable(int x, int y)
        {
            int index = y * Width + x;
            if (!CellCanHoldTile(index)) return false;
            var state = _tiles[index].State;
            return IsMatchable(state);
        }

        private void AddClearIfPassable(int x, int y, HashSet<int> clear)
        {
            if (!IsInBounds(x, y)) return;
            int index = y * Width + x;
            if (CellCanHoldTile(index)) clear.Add(index);
        }

        private void CollectDropObjects(ResolveStep step)
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (!_cells[i].Active || !_cells[i].IsDropExit || _tiles[i].Fx != 1) continue;
                var evt = new DropObjectCollectedEvent
                {
                    Coord = ToCoord(i),
                    Type = (int)_tiles[i].Type
                };
                step?.DropObjectsCollected.Add(evt);
                var tile = _tiles[i];
                tile.State = TileState.Blocker;
                tile.Fx = 0;
                _tiles[i] = tile;
            }
        }

        private bool CellCanHoldTile(int index)
        {
            if (index < 0 || index >= _cells.Length) return false;
            var cell = _cells[index];
            return cell.Active && cell.Blocker == CellBlockerType.None;
        }

        private void ApplyCellBlockerVisual(int index)
        {
            var blocker = _cells[index].Blocker;
            if (blocker == CellBlockerType.None) return;
            var tile = _tiles[index];
            tile.State = StateForBlocker(blocker);
            _tiles[index] = tile;
        }

        private Tile TileForBlockedCell(int index)
        {
            var cell = _cells[index];
            if (!cell.Active) return new Tile { State = TileState.Blocker };
            return new Tile
            {
                Type = (TileType)_rng.Next(0, 6),
                State = StateForBlocker(cell.Blocker),
                Fx = 0
            };
        }

        private static TileState StateForBlocker(CellBlockerType blocker)
        {
            switch (blocker)
            {
                case CellBlockerType.Vine: return TileState.Locked;
                case CellBlockerType.Pebble: return TileState.Pebble;
                case CellBlockerType.Ice: return TileState.Ice;
                case CellBlockerType.Moss: return TileState.Frozen;
                default: return TileState.Blocker;
            }
        }

        private BoardCoord ToCoord(int index)
        {
            return new BoardCoord(index % Width, index / Width);
        }
    }
}
