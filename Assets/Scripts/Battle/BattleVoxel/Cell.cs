using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.BattleVoxel
{
	public class Cell : VoxelKit.Cell
	{
		private List<Erelia.BattleVoxel.Type> masks = new List<Erelia.BattleVoxel.Type>();

		public IReadOnlyList<Erelia.BattleVoxel.Type> Masks => masks;

		public Cell(int id)
			: base(id)
		{
		}

		public Cell(int id, VoxelKit.Orientation orientation)
			: base(id, orientation)
		{
		}

		public Cell(int id, VoxelKit.Orientation orientation, VoxelKit.FlipOrientation flipOrientation)
			: base(id, orientation, flipOrientation)
		{
		}

		public bool HasMask(Erelia.BattleVoxel.Type mask)
		{
			return masks.Contains(mask);
		}

		public bool HasAnyMask()
		{
			return masks.Count > 0;
		}

		public void AddMask(Erelia.BattleVoxel.Type mask)
		{
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		public void RemoveMask(Erelia.BattleVoxel.Type mask)
		{
			masks.Remove(mask);
		}

		public void ClearMasks()
		{
			masks.Clear();
		}

		public static Erelia.BattleVoxel.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Erelia.BattleVoxel.Cell defaultCell = null)
		{
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			var cells = new Erelia.BattleVoxel.Cell[sizeX, sizeY, sizeZ];
			Erelia.BattleVoxel.Cell seed = defaultCell ?? new Erelia.BattleVoxel.Cell(-1);
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						var cell = new Erelia.BattleVoxel.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
						if (seed.Masks != null)
						{
							for (int m = 0; m < seed.Masks.Count; m++)
							{
								cell.AddMask(seed.Masks[m]);
							}
						}
						cells[i, j, k] = cell;
					}
				}
			}

			return cells;
		}
	}
}
