using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BoardOverlayState
{
	private readonly List<Vector3Int> borderLocalCells = new List<Vector3Int>();

	public VoxelMaskLayer MaskLayer { get; private set; } = new VoxelMaskLayer(0, 0, 0);

	public static BoardOverlayState CreateForBoard(BoardData board)
	{
		BoardOverlayState overlayState = new BoardOverlayState();
		overlayState.Initialize(board);
		return overlayState;
	}

	public void Initialize(BoardData board)
	{
		if (board?.Terrain == null)
		{
			MaskLayer = new VoxelMaskLayer(0, 0, 0);
			borderLocalCells.Clear();
			return;
		}

		MaskLayer = new VoxelMaskLayer(board.Terrain.SizeX, board.Terrain.SizeY, board.Terrain.SizeZ);
		borderLocalCells.Clear();
		if (board.BorderLocalCells != null)
		{
			borderLocalCells.AddRange(board.BorderLocalCells);
		}

		ApplyBattleAreaBorder();
	}

	public void Clear()
	{
		MaskLayer.Clear();
		ApplyBattleAreaBorder();
	}

	public void Clear(VoxelMask mask)
	{
		MaskLayer.Clear(mask);
	}

	public void ApplyMask(IReadOnlyList<Vector3Int> cells, VoxelMask mask)
	{
		if (cells == null || mask == VoxelMask.None)
		{
			return;
		}

		for (int index = 0; index < cells.Count; index++)
		{
			MaskLayer.TryAddMask(cells[index], mask);
		}
	}

	public bool TryGetMaskCell(Vector3Int localPosition, out VoxelMaskCell maskCell)
	{
		return MaskLayer.TryGetMaskCell(localPosition, out maskCell);
	}

	private void ApplyBattleAreaBorder()
	{
		for (int index = 0; index < borderLocalCells.Count; index++)
		{
			MaskLayer.TryAddMask(borderLocalCells[index], VoxelMask.BattleAreaBorder);
		}
	}
}
