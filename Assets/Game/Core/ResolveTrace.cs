using System;
using System.Collections.Generic;

namespace Game.Core
{
    [Serializable]
    public sealed class ResolveTrace
    {
        public BoardMove Swap;
        public BoardEngine.ResolveSummary Summary;
        public List<ResolveStep> Steps = new List<ResolveStep>();
        public List<SpecialCreatedEvent> SpecialsCreated = new List<SpecialCreatedEvent>();
        public List<SpecialActivatedEvent> SpecialsActivated = new List<SpecialActivatedEvent>();
        public List<BlockerHitEvent> BlockersHit = new List<BlockerHitEvent>();
        public List<BlockerClearedEvent> BlockersCleared = new List<BlockerClearedEvent>();
        public List<DropObjectCollectedEvent> DropObjectsCollected = new List<DropObjectCollectedEvent>();

        public int IterationCount => Steps != null ? Steps.Count : 0;
    }

    [Serializable]
    public struct SpecialCreatedEvent
    {
        public BoardCoord Coord;
        public TileState State;
    }

    [Serializable]
    public struct SpecialActivatedEvent
    {
        public BoardCoord Coord;
        public TileState State;
    }

    [Serializable]
    public struct BlockerHitEvent
    {
        public BoardCoord Coord;
        public CellBlockerType Type;
        public int HpAfterHit;
    }

    [Serializable]
    public struct BlockerClearedEvent
    {
        public BoardCoord Coord;
        public CellBlockerType Type;
    }

    [Serializable]
    public struct DropObjectCollectedEvent
    {
        public BoardCoord Coord;
        public int Type;
    }
}
