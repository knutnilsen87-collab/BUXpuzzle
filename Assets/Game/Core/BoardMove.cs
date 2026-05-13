using System;

namespace Game.Core
{
    [Serializable]
    public struct BoardMove
    {
        public BoardCoord A;
        public BoardCoord B;

        public BoardMove(BoardCoord a, BoardCoord b)
        {
            A = a;
            B = b;
        }

        public override string ToString()
        {
            return A + " -> " + B;
        }
    }
}
