using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Board : VoxelGrid
{
	[Serializable]
	public class BattleCell
	{
		public BattleUnit Unit;
		public List<BattleInteractiveObject> InteractiveObjects = new List<BattleInteractiveObject>();
	}

	public readonly VoxelMaskCell[,,] MaskCells;
	public readonly Dictionary<Vector3Int, BattleCell> BattleCells = new Dictionary<Vector3Int, BattleCell>();
	public readonly Dictionary<BattleObject, Vector3Int> PositionByObject = new Dictionary<BattleObject, Vector3Int>();
	public List<Vector3Int> ReachableCells = new List<Vector3Int>();

	public Board() : base(0, 0, 0)
	{
		MaskCells = new VoxelMaskCell[0, 0, 0];
	}

	public Board(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
	{
		MaskCells = new VoxelMaskCell[sizeX, sizeY, sizeZ];

		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					MaskCells[x, y, z] = new VoxelMaskCell();
				}
			}
		}
	}
}
