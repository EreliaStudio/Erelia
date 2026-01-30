using System;
using UnityEngine;

[Serializable]
public class BattlePlacementPhase : BattlePhaseBase
{
    [SerializeField, Min(1)] private int targetCellCount = 12;
    [SerializeField] private bool useBattleSeed = true;
    [SerializeField] private bool includeDiagonalNeighbors = false;

    public override BattlePhase Phase => BattlePhase.Placement;

    public override void OnEntry()
    {
        BuildPlacementMask();
    }

    public override void OnExit()
    {
        ClearPlacementMask();
    }

    public void BuildPlacementMask()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null || request.BattleBoard == null)
        {
            Debug.Log("BattlePlacementPhase: BuildPlacementMask skipped (missing request or board).");
            return;
        }

        BattleBoardData board = request.BattleBoard;
        int airId = request.Registry != null ? request.Registry.AirId : 0;
        board.ClearMask(BattleCellMask.Placement);

        System.Random rng = useBattleSeed ? new System.Random(request.Seed) : new System.Random();
        Debug.Log($"BattlePlacementPhase: BuildPlacementMask target={Mathf.Max(1, targetCellCount)} seed={request.Seed} airId={airId}.");
        ApplyRandomFloodFill(board, airId, Mathf.Max(1, targetCellCount), rng);
    }

    public void ClearPlacementMask()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null || request.BattleBoard == null)
        {
            Debug.Log("BattlePlacementPhase: ClearPlacementMask skipped (missing request or board).");
            return;
        }

        request.BattleBoard.ClearMask(BattleCellMask.Placement);
        Debug.Log("BattlePlacementPhase: Cleared placement mask.");
    }

    private void ApplyRandomFloodFill(BattleBoardData board, int airId, int targetCount, System.Random rng)
    {
        if (board == null || targetCount <= 0)
        {
            return;
        }

        var surfaceCells = new System.Collections.Generic.List<Vector2Int>();
        for (int x = 0; x < board.SizeX; x++)
        {
            for (int z = 0; z < board.SizeZ; z++)
            {
                if (board.TryGetSurfaceY(x, z, airId, out _))
                {
                    surfaceCells.Add(new Vector2Int(x, z));
                }
            }
        }

        if (surfaceCells.Count == 0)
        {
            Debug.Log("BattlePlacementPhase: No surface cells found for placement.");
            return;
        }

        Vector2Int start = surfaceCells[rng.Next(surfaceCells.Count)];
        Debug.Log($"BattlePlacementPhase: FloodFill start={start.x},{start.y} surfaces={surfaceCells.Count}.");
        var visited = new System.Collections.Generic.HashSet<Vector2Int>();
        var queue = new System.Collections.Generic.Queue<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0 && targetCount > 0)
        {
            Vector2Int cell = queue.Dequeue();
            if (board.TryGetSurfaceY(cell.x, cell.y, airId, out int surfaceY))
            {
                board.AddMask(cell.x, surfaceY, cell.y, BattleCellMask.Placement);
                Debug.Log($"BattlePlacementPhase: Placement cell ({cell.x},{surfaceY},{cell.y}).");
                targetCount--;
            }

            foreach (Vector2Int neighbor in GetNeighbors(cell, includeDiagonalNeighbors))
            {
                if (neighbor.x < 0 || neighbor.x >= board.SizeX || neighbor.y < 0 || neighbor.y >= board.SizeZ)
                {
                    continue;
                }

                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private static System.Collections.Generic.IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell, bool includeDiagonals)
    {
        yield return new Vector2Int(cell.x + 1, cell.y);
        yield return new Vector2Int(cell.x - 1, cell.y);
        yield return new Vector2Int(cell.x, cell.y + 1);
        yield return new Vector2Int(cell.x, cell.y - 1);

        if (!includeDiagonals)
        {
            yield break;
        }

        yield return new Vector2Int(cell.x + 1, cell.y + 1);
        yield return new Vector2Int(cell.x + 1, cell.y - 1);
        yield return new Vector2Int(cell.x - 1, cell.y + 1);
        yield return new Vector2Int(cell.x - 1, cell.y - 1);
    }
}
