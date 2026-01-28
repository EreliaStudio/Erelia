using System.Collections.Generic;
using UnityEngine;

public static class WorldSliceExtractor
{
    public static BattleBoardData BuildBoard(VoxelMap map, Vector3 playerWorldPosition, HashSet<Vector2Int> shapeCells, BattleAreaProfile profile)
    {
        Vector3Int originCell = Vector3Int.FloorToInt(playerWorldPosition);
        var cells = new List<BattleBoardCell>();
        int radius = profile != null ? Mathf.Max(profile.Size, profile.FillRadius) : 0;
        int verticalUp = profile != null ? profile.VerticalUp : 8;
        int verticalDown = profile != null ? profile.VerticalDown : 8;
        int fillRadius = profile != null ? profile.FillRadius : 0;

        HashSet<Vector2Int> allCells = shapeCells ?? new HashSet<Vector2Int>();
        if (fillRadius > 0)
        {
            AddCircleCells(allCells, fillRadius);
        }

        foreach (Vector2Int offset in allCells)
        {
            Vector3Int worldCell = originCell + new Vector3Int(offset.x, 0, offset.y);
            if (!TryFindTopFullVoxel(map, worldCell, verticalUp, verticalDown, out int topY, out int voxelId))
            {
                if (fillRadius <= 0)
                {
                    continue;
                }

                var emptyCell = new BattleBoardCell
                {
                    Offset = offset,
                    GroundY = originCell.y,
                    VoxelId = map != null && map.Registry != null ? map.Registry.AirId : 0,
                    Walkable = false
                };

                cells.Add(emptyCell);
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

    private static void AddCircleCells(HashSet<Vector2Int> cells, int radius)
    {
        if (cells == null || radius <= 0)
        {
            return;
        }

        int radiusSquared = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                if ((x * x) + (z * z) > radiusSquared)
                {
                    continue;
                }

                cells.Add(new Vector2Int(x, z));
            }
        }
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
