using System.Collections.Generic;
using UnityEngine;

public static class WorldSliceExtractor
{
    public static BattleBoardData BuildBoard(VoxelMap map, Vector3 playerWorldPosition, HashSet<Vector2Int> shapeCells, BattleAreaProfile profile)
    {
        Vector3Int originCell = Vector3Int.FloorToInt(playerWorldPosition);
        var cells = new List<BattleBoardCell>();
        int radius = profile != null ? profile.Size : 0;
        int verticalUp = profile != null ? profile.VerticalUp : 8;
        int verticalDown = profile != null ? profile.VerticalDown : 8;

        if (shapeCells == null)
        {
            return new BattleBoardData(originCell, radius, cells);
        }

        foreach (Vector2Int offset in shapeCells)
        {
            Vector3Int worldCell = originCell + new Vector3Int(offset.x, 0, offset.y);
            if (!TryFindTopFullVoxel(map, worldCell, verticalUp, verticalDown, out int topY, out int voxelId))
            {
                continue;
            }

            var cell = new BattleBoardCell
            {
                Offset = offset,
                GroundY = topY,
                VoxelId = voxelId,
                Walkable = true
            };

            cells.Add(cell);
        }

        return new BattleBoardData(originCell, radius, cells);
    }

    private static bool TryFindTopFullVoxel(VoxelMap map, Vector3Int baseCell, int verticalUp, int verticalDown, out int topY, out int voxelId)
    {
        topY = 0;
        voxelId = 0;
        if (map == null)
        {
            return false;
        }

        int startY = baseCell.y + Mathf.Max(0, verticalUp);
        int endY = baseCell.y - Mathf.Max(0, verticalDown);

        for (int y = startY; y >= endY; y--)
        {
            Vector3Int cell = new Vector3Int(baseCell.x, y, baseCell.z);
            if (!VoxelMapQuery.TryGetVoxel(map, cell, out Voxel voxel, out int id))
            {
                continue;
            }

            if (!VoxelMapQuery.IsFullVoxel(voxel))
            {
                continue;
            }

            topY = y;
            voxelId = id;
            return true;
        }

        return false;
    }
}
