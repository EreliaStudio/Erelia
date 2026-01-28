using System.Collections.Generic;
using UnityEngine;

public static class WorldSliceExtractor
{
    public static BattleBoard BuildBattleBoard(VoxelMap map, Vector3 playerWorldPosition, HashSet<Vector2Int> shapeCells, BattleAreaProfile profile)
    {
        Vector3Int originCell = Vector3Int.FloorToInt(playerWorldPosition);
        int radius = profile != null ? Mathf.Max(1, profile.Size) : 0;
        int verticalUp = profile != null ? profile.VerticalUp : 8;
        int verticalDown = profile != null ? profile.VerticalDown : 8;

        int sizeX = radius * 2 + 1;
        int sizeZ = radius * 2 + 1;
        int sizeY = verticalUp + verticalDown + 1;
        Vector3Int boardOrigin = new Vector3Int(originCell.x - radius, originCell.y - verticalDown, originCell.z - radius);
        var board = new BattleBoard(boardOrigin, sizeX, sizeY, sizeZ);

        if (radius <= 0 || sizeY <= 0)
        {
            return board;
        }

        HashSet<Vector2Int> allCells = shapeCells;
        if (allCells == null || allCells.Count == 0)
        {
            allCells = new HashSet<Vector2Int>();
            AddSquareCells(allCells, radius);
        }

        int airId = map != null && map.Registry != null ? map.Registry.AirId : 0;
        VoxelCell airCell = new VoxelCell(airId);
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    board.Voxels[x, y, z] = airCell;
                }
            }
        }

        foreach (Vector2Int offset in allCells)
        {
            if (Mathf.Abs(offset.x) > radius || Mathf.Abs(offset.y) > radius)
            {
                continue;
            }

            int localX = offset.x + radius;
            int localZ = offset.y + radius;
            for (int y = 0; y < sizeY; y++)
            {
                Vector3Int worldCell = new Vector3Int(boardOrigin.x + localX, boardOrigin.y + y, boardOrigin.z + localZ);
                if (VoxelMapQuery.TryGetVoxelCell(map, worldCell, out VoxelCell cell))
                {
                    board.Voxels[localX, y, localZ] = cell;
                }
            }
        }

        return board;
    }

    private static void AddSquareCells(HashSet<Vector2Int> cells, int radius)
    {
        if (cells == null || radius <= 0)
        {
            return;
        }

        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                cells.Add(new Vector2Int(x, z));
            }
        }
    }
}
