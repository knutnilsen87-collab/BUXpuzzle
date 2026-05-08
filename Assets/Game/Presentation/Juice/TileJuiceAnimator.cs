using System.Collections;
using Game.Core;
using UnityEngine;

namespace Game.Presentation.Juice
{
    public sealed class TileJuiceAnimator : MonoBehaviour
    {
        public IEnumerator PulseMatched(BoardView board, ResolveStep step)
        {
            if (board == null || step == null) yield break;

            board.SetTilesResolveHighlight(step.Matched, true);
            yield return new WaitForSeconds(0.15f);
            board.SetTilesResolveHighlight(step.Matched, false);
        }

        public IEnumerator ClearMatched(BoardView board, ResolveStep step)
        {
            if (board == null || step == null) yield break;

            board.SetTilesClearing(step.Cleared, true);
            yield return new WaitForSeconds(0.22f);
            board.SetTilesClearing(step.Cleared, false);
            yield return new WaitForSeconds(0.08f);
        }

        public IEnumerator Land(TileView[] tiles)
        {
            if (tiles == null) yield break;

            foreach (var tile in tiles)
            {
                if (tile != null) tile.SetLanding(true);
            }

            yield return new WaitForSeconds(0.10f);

            foreach (var tile in tiles)
            {
                if (tile != null) tile.SetLanding(false);
            }
        }
    }
}
