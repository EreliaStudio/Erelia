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
		[SerializeField] private Erelia.Core.Creature.Team enemyTeam;

		/// <summary>
		/// Coordinates that are acceptable floor candidates for later phases.
		/// </summary>
		public IReadOnlyList<Vector3Int> AcceptableCoordinates => acceptableCoordinates;
		public IReadOnlyList<Vector3Int> PlayerPlacementCoordinates => playerPlacementCoordinates;
		public IReadOnlyList<Vector3Int> EnemyPlacementCoordinates => enemyPlacementCoordinates;
		public Erelia.Core.Creature.Team EnemyTeam => enemyTeam;

		/// <summary>
		/// Clears all cached setup data.
		/// </summary>
		public void Clear()
		{
			acceptableCoordinates.Clear();
			playerPlacementCoordinates.Clear();
			enemyPlacementCoordinates.Clear();
			enemyTeam = null;
		}

		public void AddAcceptableCoordinates(List<Vector3Int> coordinates)
		{
			if (coordinates == null)
			{
				return;
			}

			acceptableCoordinates.AddRange(coordinates);
		}

		public void AddPlayerPlacementCoordinates(List<Vector3Int> coordinates)
		{
			if (coordinates == null)
			{
				return;
			}

			playerPlacementCoordinates.AddRange(coordinates);
		}

		public void AddEnemyPlacementCoordinates(List<Vector3Int> coordinates)
		{
			if (coordinates == null)
			{
				return;
			}

			enemyPlacementCoordinates.AddRange(coordinates);
		}

		public void SetEnemyTeam(Erelia.Core.Creature.Team team)
		{
			enemyTeam = team;
		}
	}
}
