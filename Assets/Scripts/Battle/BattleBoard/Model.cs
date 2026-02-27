using UnityEngine;

namespace Erelia.Battle.Board
{
	public sealed class Model
	{
		public Erelia.BattleVoxel.Cell[,,] Cells { get; }
		public Vector3Int Origin { get; }
		public Vector3Int Center { get; }

		public int SizeX => Cells?.GetLength(0) ?? 0;
		public int SizeY => Cells?.GetLength(1) ?? 0;
		public int SizeZ => Cells?.GetLength(2) ?? 0;

		public Model(Erelia.BattleVoxel.Cell[,,] cells, Vector3Int origin, Vector3Int center)
		{
			Cells = cells;
			Origin = origin;
			Center = center;
		}
	}
}
