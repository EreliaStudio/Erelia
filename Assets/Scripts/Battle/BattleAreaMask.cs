using System.Collections.Generic;
using UnityEngine;

public class BattleAreaMask
{
    private readonly Vector3Int originCell;
    private readonly HashSet<Vector2Int> offsets;

    public BattleAreaMask(Vector3Int originCellValue, HashSet<Vector2Int> offsetsValue)
    {
        originCell = originCellValue;
        offsets = offsetsValue ?? new HashSet<Vector2Int>();
    }

    public static BattleAreaMask FromBoard(BattleBoardData board)
    {
        if (board == null)
        {
            return null;
        }

        var offsetSet = new HashSet<Vector2Int>();
        IReadOnlyList<BattleBoardCell> cells = board.Cells;
        for (int i = 0; i < cells.Count; i++)
        {
            offsetSet.Add(cells[i].Offset);
        }

        return new BattleAreaMask(board.OriginCell, offsetSet);
    }

    public bool ContainsWorldXZ(int worldX, int worldZ)
    {
        Vector2Int offset = new Vector2Int(worldX - originCell.x, worldZ - originCell.z);
        return offsets.Contains(offset);
    }
}
