using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class VoxelMaskLayer
{
	private readonly int sizeX;
	private readonly int sizeY;
	private readonly int sizeZ;

	private readonly Dictionary<Vector3Int, VoxelMaskCell> activeCells = new Dictionary<Vector3Int, VoxelMaskCell>();

	public int ActiveCellCount => activeCells.Count;
	internal Dictionary<Vector3Int, VoxelMaskCell> ActiveCells => activeCells;

	public VoxelMaskLayer(int p_sizeX, int p_sizeY, int p_sizeZ)
	{
		sizeX = Mathf.Max(0, p_sizeX);
		sizeY = Mathf.Max(0, p_sizeY);
		sizeZ = Mathf.Max(0, p_sizeZ);
	}

	public bool TryAddMask(Vector3Int p_localPosition, VoxelMask p_mask)
	{
		if (!IsInside(p_localPosition) || p_mask == VoxelMask.None)
		{
			return false;
		}

		if (!activeCells.TryGetValue(p_localPosition, out VoxelMaskCell maskCell) || maskCell == null)
		{
			maskCell = new VoxelMaskCell();
			activeCells[p_localPosition] = maskCell;
		}

		if (maskCell.Masks.Contains(p_mask))
		{
			return false;
		}

		maskCell.Masks.Add(p_mask);
		return true;
	}

	public bool TryRemoveMask(Vector3Int p_localPosition, VoxelMask p_mask)
	{
		if (!IsInside(p_localPosition) || p_mask == VoxelMask.None)
		{
			return false;
		}

		if (!activeCells.TryGetValue(p_localPosition, out VoxelMaskCell maskCell) || maskCell == null)
		{
			return false;
		}

		bool removed = maskCell.Masks.Remove(p_mask);
		if (maskCell.Masks.Count == 0)
		{
			activeCells.Remove(p_localPosition);
		}

		return removed;
	}

	public bool TryGetMaskCell(Vector3Int p_localPosition, out VoxelMaskCell p_maskCell)
	{
		if (!IsInside(p_localPosition))
		{
			p_maskCell = null;
			return false;
		}

		return activeCells.TryGetValue(p_localPosition, out p_maskCell) && p_maskCell != null;
	}

	public void Clear()
	{
		activeCells.Clear();
	}

	private bool IsInside(Vector3Int p_localPosition)
	{
		return p_localPosition.x >= 0 && p_localPosition.x < sizeX &&
		       p_localPosition.y >= 0 && p_localPosition.y < sizeY &&
		       p_localPosition.z >= 0 && p_localPosition.z < sizeZ;
	}
}
