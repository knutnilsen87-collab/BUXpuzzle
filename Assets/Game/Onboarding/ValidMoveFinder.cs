using Game.Core;

namespace Game.Onboarding
{
    public static class ValidMoveFinder
    {
        public static bool TryFind(BoardEngine board, out BoardMove move)
        {
            move = default(BoardMove);
            if (board == null) return false;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (x + 1 < board.Width && board.WouldSwapCreateMatch(x, y, x + 1, y))
                    {
                        move = new BoardMove(new BoardCoord(x, y), new BoardCoord(x + 1, y));
                        return true;
                    }

                    if (y + 1 < board.Height && board.WouldSwapCreateMatch(x, y, x, y + 1))
                    {
                        move = new BoardMove(new BoardCoord(x, y), new BoardCoord(x, y + 1));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
