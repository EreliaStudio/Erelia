using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Placement
{
	public static class PlacementCoordinateGenerator
	{
		public static bool TryGenerate(
			Erelia.Battle.Board.Model board,
			IReadOnlyList<Vector3Int> acceptableCoordinates,
			Erelia.Battle.Phase.Placement.PlacementMode mode,
			out List<Vector3Int> playerCoordinates,
			out List<Vector3Int> enemyCoordinates)
		{
			playerCoordinates = new List<Vector3Int>();
			enemyCoordinates = new List<Vector3Int>();

			if (board == null || acceptableCoordinates == null || acceptableCoordinates.Count == 0)
			{
				return false;
			}

			switch (mode)
			{
				case Erelia.Battle.Phase.Placement.PlacementMode.HalfBoard:
					GenerateHalfBoardLists(board, acceptableCoordinates, playerCoordinates, enemyCoordinates);
					break;
				default:
					return false;
			}

			return playerCoordinates.Count > 0 && enemyCoordinates.Count > 0;
		}

		private static void GenerateHalfBoardLists(
			Erelia.Battle.Board.Model board,
			IReadOnlyList<Vector3Int> acceptableCoordinates,
			List<Vector3Int> playerCoordinates,
			List<Vector3Int> enemyCoordinates)
		{
			int splitZ = board.SizeZ / 2;
			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (coordinate.z < splitZ)
				{
					playerCoordinates.Add(coordinate);
				}
				else
				{
					enemyCoordinates.Add(coordinate);
				}
			}
		}
	}
}
