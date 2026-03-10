using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase
{
	/// <summary>
	/// Shared phase-specific runtime info populated across the battle flow.
	/// </summary>
	[System.Serializable]
	public sealed class Info
	{
		[SerializeField] private List<Vector3Int> acceptableCoordinates = new List<Vector3Int>();
		[SerializeField] private List<Vector3Int> playerPlacementCoordinates = new List<Vector3Int>();
		[SerializeField] private List<Vector3Int> enemyPlacementCoordinates = new List<Vector3Int>();

		public IReadOnlyList<Vector3Int> AcceptableCoordinates => acceptableCoordinates;
		public IReadOnlyList<Vector3Int> PlayerPlacementCoordinates => playerPlacementCoordinates;
		public IReadOnlyList<Vector3Int> EnemyPlacementCoordinates => enemyPlacementCoordinates;

		public void Clear()
		{
			acceptableCoordinates.Clear();
			playerPlacementCoordinates.Clear();
			enemyPlacementCoordinates.Clear();
		}

		public void ClearPlacementCoordinates()
		{
			playerPlacementCoordinates.Clear();
			enemyPlacementCoordinates.Clear();
		}

		public void SetAcceptableCoordinates(IEnumerable<Vector3Int> coordinates)
		{
			acceptableCoordinates.Clear();
			AddRange(acceptableCoordinates, coordinates);
		}

		public void SetPlayerPlacementCoordinates(IEnumerable<Vector3Int> coordinates)
		{
			playerPlacementCoordinates.Clear();
			AddRange(playerPlacementCoordinates, coordinates);
		}

		public void SetEnemyPlacementCoordinates(IEnumerable<Vector3Int> coordinates)
		{
			enemyPlacementCoordinates.Clear();
			AddRange(enemyPlacementCoordinates, coordinates);
		}

		private static void AddRange(List<Vector3Int> target, IEnumerable<Vector3Int> coordinates)
		{
			if (target == null || coordinates == null)
			{
				return;
			}

			foreach (Vector3Int coordinate in coordinates)
			{
				target.Add(coordinate);
			}
		}
	}
}
