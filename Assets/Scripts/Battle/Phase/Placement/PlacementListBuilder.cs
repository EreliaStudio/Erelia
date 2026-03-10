using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Placement
{
	public static class PlacementListBuilder
	{
		public static bool TryBuildFromAcceptableCoordinates(
			IReadOnlyList<Vector3Int> acceptableCoordinates,
			out List<Vector3Int> playerCoordinates,
			out List<Vector3Int> enemyCoordinates)
		{
			playerCoordinates = new List<Vector3Int>();
			enemyCoordinates = new List<Vector3Int>();

			if (acceptableCoordinates == null || acceptableCoordinates.Count == 0)
			{
				return false;
			}

			int minZ = int.MaxValue;
			int maxZ = int.MinValue;
			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (coordinate.z < minZ)
				{
					minZ = coordinate.z;
				}

				if (coordinate.z > maxZ)
				{
					maxZ = coordinate.z;
				}
			}

			int splitZ = minZ + ((maxZ - minZ + 1) / 2);
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

			return playerCoordinates.Count > 0 && enemyCoordinates.Count > 0;
		}
	}
}
