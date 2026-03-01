using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	public class Cell : Erelia.Core.VoxelKit.Cell
	{
		private List<Erelia.Battle.Voxel.Type> masks = new List<Erelia.Battle.Voxel.Type>();

		public IReadOnlyList<Erelia.Battle.Voxel.Type> Masks => masks;

		public Cell(int id)
			: base(id)
		{
		}

		public Cell(int id, Erelia.Core.VoxelKit.Orientation orientation)
			: base(id, orientation)
		{
		}

		public Cell(int id, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
			: base(id, orientation, flipOrientation)
		{
		}

		public bool HasMask(Erelia.Battle.Voxel.Type mask)
		{
			return masks.Contains(mask);
		}

		public bool HasAnyMask()
		{
			return masks.Count > 0;
		}

		public void AddMask(Erelia.Battle.Voxel.Type mask)
		{
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		public void RemoveMask(Erelia.Battle.Voxel.Type mask)
		{
			masks.Remove(mask);
		}

		public void ClearMasks()
		{
			masks.Clear();
		}

		public static Erelia.Battle.Voxel.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Erelia.Battle.Voxel.Cell defaultCell = null)
		{
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			var cells = new Erelia.Battle.Voxel.Cell[sizeX, sizeY, sizeZ];
			Erelia.Battle.Voxel.Cell seed = defaultCell ?? new Erelia.Battle.Voxel.Cell(-1);
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						var cell = new Erelia.Battle.Voxel.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
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
