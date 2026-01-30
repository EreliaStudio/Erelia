using System;
using UnityEngine;

[Serializable]
public class BattlePlacementPhase : BattlePhaseBase
{
    [SerializeField, Min(1)] private int targetCellCount = 12;
    [SerializeField] private bool useBattleSeed = true;

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
            return;
        }

        BattleBoardData board = request.BattleBoard;
        int airId = request.Registry != null ? request.Registry.AirId : 0;
        board.ClearMask(BattleCellMask.Placement);

        System.Random rng = useBattleSeed ? new System.Random(request.Seed) : new System.Random();
        int desiredCellCount = targetCellCount;
        if (request.AreaProfile != null)
        {
            desiredCellCount = request.AreaProfile.PlacementCellCount;
        }

        ApplyRandomFloodFill(board, airId, Mathf.Max(1, desiredCellCount), rng);
        battleContext?.BattleBoard?.RebuildMask();
    }

    public void ClearPlacementMask()
    {
        BattleRequest request = BattleRequestStore.Current;
        if (request == null || request.BattleBoard == null)
        {
            return;
        }

        request.BattleBoard.ClearMask(BattleCellMask.Placement);
        battleContext?.BattleBoard?.RebuildMask();
    }

    private void ApplyRandomFloodFill(BattleBoardData board, int airId, int targetCount, System.Random rng)
    {
        if (board == null || targetCount <= 0)
        {
            return;
        }

        var surfaceCells = new System.Collections.Generic.List<Vector2Int>();
        var surfaceHeights = new System.Collections.Generic.Dictionary<Vector2Int, int>();
        for (int x = 0; x < board.SizeX; x++)
        {
            for (int z = 0; z < board.SizeZ; z++)
            {
                if (TryGetPlacementY(board, x, z, airId, out int placementY))
                {
                    Vector2Int cell = new Vector2Int(x, z);
                    surfaceCells.Add(cell);
                    surfaceHeights[cell] = placementY;
                }
            }
        }

        if (surfaceCells.Count == 0)
        {
            return;
        }

        Vector2Int start = surfaceCells[rng.Next(surfaceCells.Count)];
        var visited = new System.Collections.Generic.HashSet<Vector2Int>();
        var queue = new System.Collections.Generic.Queue<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0 && targetCount > 0)
        {
            Vector2Int cell = queue.Dequeue();
            if (surfaceHeights.TryGetValue(cell, out int placementY))
            {
                board.AddMask(cell.x, placementY, cell.y, BattleCellMask.Placement);
                targetCount--;
            }

            foreach (Vector2Int neighbor in GetNeighbors(cell))
            {
                if (neighbor.x < 0 || neighbor.x >= board.SizeX || neighbor.y < 0 || neighbor.y >= board.SizeZ)
                {
                    continue;
                }

                if (!surfaceHeights.ContainsKey(neighbor))
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

    private static bool TryGetPlacementY(BattleBoardData board, int x, int z, int airId, out int placementY)
    {
        placementY = -1;
        if (board == null)
        {
            return false;
        }

        if (!board.TryGetSurfaceY(x, z, airId, out int surfaceAirY))
        {
            return false;
        }

        placementY = surfaceAirY;
        return true;
    }

    private static readonly Vector2Int[] NeighborOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private static System.Collections.Generic.IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        for (int i = 0; i < NeighborOffsets.Length; i++)
        {
            Vector2Int offset = NeighborOffsets[i];
            yield return new Vector2Int(cell.x + offset.x, cell.y + offset.y);
        }
    }
}
