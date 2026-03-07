using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Board
{
	/// <summary>
	/// Data model for the battle board voxel grid.
	/// Holds the cells plus origin/center metadata and parsed playable cells for placement and rendering.
	/// </summary>
	public sealed class Model
	{
		/// <summary>
		/// 3D grid of battle voxel cells.
		/// </summary>
		public Erelia.Battle.Voxel.Cell[,,] Cells { get; }
		/// <summary>
		/// World-space origin of the board grid.
		/// </summary>
		public Vector3Int Origin { get; }
		/// <summary>
		/// World-space center cell of the board grid.
		/// </summary>
		public Vector3Int Center { get; }
		/// <summary>
		/// Board size on the X axis.
		/// </summary>
		public int SizeX => Cells?.GetLength(0) ?? 0;
		/// <summary>
		/// Board size on the Y axis.
		/// </summary>
		public int SizeY => Cells?.GetLength(1) ?? 0;
		/// <summary>
		/// Board size on the Z axis.
		/// </summary>
		public int SizeZ => Cells?.GetLength(2) ?? 0;

		/// <summary>
		/// Creates a board model with cells and origin/center metadata.
		/// </summary>
		public Model(
			Erelia.Battle.Voxel.Cell[,,] cells,
			Vector3Int origin,
			Vector3Int center)
		{
			// Store the board data.
			Cells = cells;
			Origin = origin;
			Center = center;
		}
	}
}
