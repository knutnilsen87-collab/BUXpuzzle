using System.Collections;
using Game.Core;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class BoardResolveAnimator : MonoBehaviour
    {
        public IEnumerator Play(BoardView board, TileView a, TileView b, ResolveTrace trace)
        {
            if (board == null || trace == null)
            {
                yield break;
            }

            yield return AnimateAcceptedSwap(board, a, b);

            foreach (var step in trace.Steps)
            {
                board.SetTilesResolveHighlight(step.Matched, true);
                yield return new WaitForSeconds(0.18f);

                board.SetTilesClearing(step.Cleared, true);
                yield return new WaitForSeconds(0.16f);

                board.SetTilesClearing(step.Cleared, false);
                board.SetTilesResolveHighlight(step.Matched, false);

                if (step.Drops.Count > 0)
                {
                    yield return AnimateDrops(board, step);
                }

                if (step.Spawned.Count > 0)
                {
                    board.PulseSpawnedTiles(step.Spawned);
                    yield return new WaitForSeconds(0.16f);
                }

                if (trace.Steps.Count > 1)
                {
                    yield return new WaitForSeconds(0.10f);
                }
            }
        }

        private IEnumerator AnimateAcceptedSwap(BoardView board, TileView a, TileView b)
        {
            if (a == null || b == null) yield break;

            Vector3 a0 = a.transform.localPosition;
            Vector3 b0 = b.transform.localPosition;
            const float duration = 0.16f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                a.transform.localPosition = Vector3.Lerp(a0, b0, k);
                b.transform.localPosition = Vector3.Lerp(b0, a0, k);
                yield return null;
            }

            a.transform.localPosition = b0;
            b.transform.localPosition = a0;
            board.SwapVisualSlots(a, b);
        }

        private IEnumerator AnimateDrops(BoardView board, ResolveStep step)
        {
            const float duration = 0.18f;
            float t = 0f;
            var tiles = new TileView[step.Drops.Count];
            var starts = new Vector3[step.Drops.Count];
            var ends = new Vector3[step.Drops.Count];

            for (int i = 0; i < step.Drops.Count; i++)
            {
                var drop = step.Drops[i];
                tiles[i] = board.GetTileView(drop.From.X, drop.From.Y);
                if (tiles[i] == null) continue;
                starts[i] = tiles[i].transform.localPosition;
                ends[i] = board.LocalPositionFor(drop.To.X, drop.To.Y);
            }

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                for (int i = 0; i < tiles.Length; i++)
                {
                    if (tiles[i] != null) tiles[i].transform.localPosition = Vector3.Lerp(starts[i], ends[i], k);
                }

                yield return null;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null) tiles[i].transform.localPosition = ends[i];
            }

            board.ApplyDropVisualSlots(step.Drops);
            yield return new WaitForSeconds(0.08f);
        }
    }
}
