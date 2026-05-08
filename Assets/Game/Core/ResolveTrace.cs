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

        public int IterationCount => Steps != null ? Steps.Count : 0;
    }
}
