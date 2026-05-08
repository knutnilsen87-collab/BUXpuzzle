using System;
using System.Collections.Generic;

namespace Game.Levels
{
    public static class DeterministicEndlessLevelGenerator
    {
        public static int SeedForLevel(int levelIndex)
        {
            unchecked
            {
                uint x = (uint)(levelIndex + 1) * 2654435761u;
                x ^= x >> 16;
                x *= 2246822519u;
                x ^= x >> 13;
                x *= 3266489917u;
                x ^= x >> 16;
                return (int)x;
            }
        }

        public static int[,] GenerateBoard(int width, int height, int tileKindCount, int levelIndex)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (tileKindCount < 3) throw new ArgumentOutOfRangeException(nameof(tileKindCount), "Need at least 3 tile kinds.");

            var seed = SeedForLevel(levelIndex);
            var rng = new Random(seed);
            var board = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var candidates = new List<int>(tileKindCount);
                    for (int k = 0; k < tileKindCount; k++)
                    {
                        if (CreatesInitialMatch(board, x, y, k)) continue;
                        candidates.Add(k);
                    }

                    if (candidates.Count == 0)
                    {
                        for (int k = 0; k < tileKindCount; k++) candidates.Add(k);
                    }

                    board[x, y] = candidates[rng.Next(candidates.Count)];
                }
            }

            if (!HasLegalMove(board, width, height))
{
    ForceSimpleLegalMove(board, width, height);
    if (!HasLegalMove(board, width, height))
    {
        throw new InvalidOperationException("DeterministicEndlessLevelGenerator failed to produce a legal move after fallback repair.");
    }
}

return board;
        }

        private static bool CreatesInitialMatch(int[,] board, int x, int y, int value)
        {
            if (x >= 2 && board[x - 1, y] == value && board[x - 2, y] == value) return true;
            if (y >= 2 && board[x, y - 1] == value && board[x, y - 2] == value) return true;
            return false;
        }

        private static bool HasLegalMove(int[,] board, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x + 1 < width)
                    {
                        Swap(board, x, y, x + 1, y);
                        bool ok = HasAnyMatch(board, width, height);
                        Swap(board, x, y, x + 1, y);
                        if (ok) return true;
                    }

                    if (y + 1 < height)
                    {
                        Swap(board, x, y, x, y + 1);
                        bool ok = HasAnyMatch(board, width, height);
                        Swap(board, x, y, x, y + 1);
                        if (ok) return true;
                    }
                }
            }
            return false;
        }

        private static void ForceSimpleLegalMove(int[,] board, int width, int height)
        {
            if (width < 3 || height < 2) return;

            int a = board[0, 0];
            int b = (a + 1) % Math.Max(3, GetKindCountEstimate(board, width, height));
            board[0, 0] = a;
            board[1, 0] = b;
            board[2, 0] = a;
            board[1, 1] = a;
        }

        private static int GetKindCountEstimate(int[,] board, int width, int height)
        {
            var set = new HashSet<int>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    set.Add(board[x, y]);
            return Math.Max(3, set.Count);
        }

        private static bool HasAnyMatch(int[,] board, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                int streak = 1;
                for (int x = 1; x < width; x++)
                {
                    streak = (board[x, y] == board[x - 1, y]) ? streak + 1 : 1;
                    if (streak >= 3) return true;
                }
            }

            for (int x = 0; x < width; x++)
            {
                int streak = 1;
                for (int y = 1; y < height; y++)
                {
                    streak = (board[x, y] == board[x, y - 1]) ? streak + 1 : 1;
                    if (streak >= 3) return true;
                }
            }

            return false;
        }

        private static void Swap(int[,] board, int x1, int y1, int x2, int y2)
        {
            int tmp = board[x1, y1];
            board[x1, y1] = board[x2, y2];
            board[x2, y2] = tmp;
        }
    }
}

