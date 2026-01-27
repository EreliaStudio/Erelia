using System;
using UnityEngine;

[Serializable]
public struct BattleBoardCell
{
    public Vector2Int Offset;
    public int GroundY;
    public int VoxelId;
    public bool Walkable;
}
