using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Placement
{
	public static class PlacementListBuilder
	{
		public static bool TryBuildFromAcceptableCoordinates(
			IReadOnlyList<Vector3Int> acceptableCoordinates,
			int boardSizeZ,
			out List<Vector3Int> playerCoordinates,
			out List<Vector3Int> enemyCoordinates)
		{
			playerCoordinates = new List<Vector3Int>();
			enemyCoordinates = new List<Vector3Int>();

			if (acceptableCoordinates == null || acceptableCoordinates.Count == 0 || boardSizeZ <= 0)
			{
				return false;
			}

			int splitZ = Mathf.Clamp(boardSizeZ / 2, 1, boardSizeZ);
			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (coordinate.z < splitZ)
				{
					playerCoordinates.Add(coordinate);
					continue;
				}

				enemyCoordinates.Add(coordinate);
			}

			if (playerCoordinates.Count > 0 && enemyCoordinates.Count > 0)
			{
				return true;
			}

			RebuildBySortedHalf(acceptableCoordinates, playerCoordinates, enemyCoordinates);
			return playerCoordinates.Count > 0 && enemyCoordinates.Count > 0;
		}

		private static void RebuildBySortedHalf(
			IReadOnlyList<Vector3Int> acceptableCoordinates,
			List<Vector3Int> playerCoordinates,
			List<Vector3Int> enemyCoordinates)
		{
			playerCoordinates.Clear();
			enemyCoordinates.Clear();

			var sortedCoordinates = new List<Vector3Int>(acceptableCoordinates);
			sortedCoordinates.Sort(CompareCoordinates);

			int playerCount = sortedCoordinates.Count > 1
				? sortedCoordinates.Count / 2
				: 1;
			for (int i = 0; i < sortedCoordinates.Count; i++)
			{
				if (i < playerCount)
				{
					playerCoordinates.Add(sortedCoordinates[i]);
					continue;
				}

				enemyCoordinates.Add(sortedCoordinates[i]);
			}
		}

		private static int CompareCoordinates(Vector3Int left, Vector3Int right)
		{
			int zComparison = left.z.CompareTo(right.z);
			if (zComparison != 0)
			{
				return zComparison;
			}

			int xComparison = left.x.CompareTo(right.x);
			if (xComparison != 0)
			{
				return xComparison;
			}

			return left.y.CompareTo(right.y);
		}
	}
}
