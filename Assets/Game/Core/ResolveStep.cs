using System;
using System.Collections.Generic;

namespace Game.Core
{
    [Serializable]
    public struct DropMove
    {
        public BoardCoord From;
        public BoardCoord To;
        public int Type;
    }

    [Serializable]
    public struct SpawnedTile
    {
        public BoardCoord Coord;
        public int Type;
    }

    [Serializable]
    public sealed class ResolveStep
    {
        public int Iteration;
        public List<BoardCoord> Matched = new List<BoardCoord>();
        public List<BoardCoord> Cleared = new List<BoardCoord>();
        public List<DropMove> Drops = new List<DropMove>();
        public List<SpawnedTile> Spawned = new List<SpawnedTile>();
        public List<DropObjectCollectedEvent> DropObjectsCollected = new List<DropObjectCollectedEvent>();
    }
}
