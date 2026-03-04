using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	/// <summary>
	/// Battle voxel cell with mask support on top of base voxel data.
	/// Adds mask management helpers and pack creation for battle boards.
	/// </summary>
	public class Cell : Erelia.Core.VoxelKit.Cell
	{
		/// <summary>
		/// List of masks applied to this cell.
		/// </summary>
		private List<Erelia.Battle.Voxel.Mask.Type> masks = new List<Erelia.Battle.Voxel.Mask.Type>();

		/// <summary>
		/// Gets the read-only list of masks applied to the cell.
		/// </summary>
		public IReadOnlyList<Erelia.Battle.Voxel.Mask.Type> Masks => masks;

		/// <summary>
		/// Creates a battle voxel cell with the given id.
		/// </summary>
		public Cell(int id)
			: base(id)
		{
			// Base constructor handles id initialization.
		}

		/// <summary>
		/// Creates a battle voxel cell with an id and orientation.
		/// </summary>
		public Cell(int id, Erelia.Core.VoxelKit.Orientation orientation)
			: base(id, orientation)
		{
			// Base constructor handles orientation initialization.
		}

		/// <summary>
		/// Creates a battle voxel cell with id, orientation, and flip orientation.
		/// </summary>
		public Cell(int id, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
			: base(id, orientation, flipOrientation)
		{
			// Base constructor handles orientation initialization.
		}

		/// <summary>
		/// Checks whether the cell contains the given mask.
		/// </summary>
		public bool HasMask(Erelia.Battle.Voxel.Mask.Type mask)
		{
			// Look for the mask in the list.
			return masks.Contains(mask);
		}

		/// <summary>
		/// Checks whether the cell has any masks assigned.
		/// </summary>
		public bool HasAnyMask()
		{
			// Return true if any masks exist.
			return masks.Count > 0;
		}

		/// <summary>
		/// Adds a mask to the cell if not already present.
		/// </summary>
		public void AddMask(Erelia.Battle.Voxel.Mask.Type mask)
		{
			// Prevent duplicate masks.
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		/// <summary>
		/// Removes a mask from the cell.
		/// </summary>
		public void RemoveMask(Erelia.Battle.Voxel.Mask.Type mask)
		{
			// Remove the mask if present.
			masks.Remove(mask);
		}

		/// <summary>
		/// Clears all masks from the cell.
		/// </summary>
		public void ClearMasks()
		{
			// Remove all masks.
			masks.Clear();
		}

		/// <summary>
		/// Creates a 3D pack of battle cells initialized from a seed cell.
		/// </summary>
		public static Erelia.Battle.Voxel.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Erelia.Battle.Voxel.Cell defaultCell = null)
		{
			// Allocate and fill a cell grid using the seed cell values.
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
