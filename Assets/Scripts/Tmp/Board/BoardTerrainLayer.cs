using System;
using UnityEngine;

[Serializable]
public sealed class BoardTerrainLayer : VoxelGrid
{
	public readonly VoxelMaskLayer MaskLayer;

	[NonSerialized]
	private VoxelRegistry _voxelRegistry;

	public VoxelRegistry VoxelRegistry => _voxelRegistry;

	public BoardTerrainLayer() : this(0, 0, 0)
	{
	}

	public BoardTerrainLayer(int p_sizeX, int p_sizeY, int p_sizeZ) : base(p_sizeX, p_sizeY, p_sizeZ)
	{
		MaskLayer = new VoxelMaskLayer(p_sizeX, p_sizeY, p_sizeZ);
	}

	public void AssignVoxelRegistry(VoxelRegistry p_voxelRegistry)
	{
		_voxelRegistry = p_voxelRegistry;
	}

	public bool IsInside(Vector3Int p_position)
	{
		return IsWithinBounds(p_position);
	}

	public bool TryGetCell(Vector3Int p_position, out VoxelCell p_cell)
	{
		if (!IsWithinBounds(p_position))
		{
			p_cell = null;
			return false;
		}

		p_cell = Cells[p_position.x, p_position.y, p_position.z];
		return true;
	}

	public bool HasTerrain(Vector3Int p_position)
	{
		return TryGetCell(p_position, out VoxelCell cell) && cell != null && cell.IsEmpty == false;
	}

	public bool IsEmpty(Vector3Int p_position)
	{
		return !TryGetCell(p_position, out VoxelCell cell) || cell == null || cell.IsEmpty;
	}
}
