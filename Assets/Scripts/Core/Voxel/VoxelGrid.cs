using System;
using UnityEngine;

[Serializable]
public class VoxelGrid
{
	public readonly VoxelCell[,,] Cells;

	public int SizeX => Cells.GetLength(0);
	public int SizeY => Cells.GetLength(1);
	public int SizeZ => Cells.GetLength(2);

	public VoxelGrid() : this(0, 0, 0)
	{
	}

	public VoxelGrid(int sizeX, int sizeY, int sizeZ)
	{
		Cells = new VoxelCell[sizeX, sizeY, sizeZ];
	}

	public bool IsWithinBounds(Vector3Int p_position)
	{
		return p_position.x >= 0 && p_position.x < SizeX &&
			p_position.y >= 0 && p_position.y < SizeY &&
			p_position.z >= 0 && p_position.z < SizeZ;
	}
}
