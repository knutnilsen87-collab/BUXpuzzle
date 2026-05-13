using System;

namespace Game.Core
{
    [Serializable]
    public struct BoardCoord
    {
        public int X;
        public int Y;

        public BoardCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return X + "," + Y;
        }
    }
}
