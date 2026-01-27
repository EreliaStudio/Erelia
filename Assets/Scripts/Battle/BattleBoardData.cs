using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleBoardData
{
    [SerializeField] private Vector3Int originCell;
    [SerializeField] private int radius;
    [SerializeField] private List<BattleBoardCell> cells = new List<BattleBoardCell>();

    public Vector3Int OriginCell => originCell;
    public int Radius => radius;
    public IReadOnlyList<BattleBoardCell> Cells => cells;

    public BattleBoardData(Vector3Int originCellValue, int radiusValue, List<BattleBoardCell> cellsValue)
    {
        originCell = originCellValue;
        radius = radiusValue;
        cells = cellsValue ?? new List<BattleBoardCell>();
    }
}
