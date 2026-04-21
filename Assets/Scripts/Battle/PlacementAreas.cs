using System.Collections.Generic;
using UnityEngine;

public sealed class PlacementAreas
{
	public readonly IReadOnlyList<Vector3Int> PlayerCells;
	public readonly IReadOnlyList<Vector3Int> EnemyCells;

	public PlacementAreas(List<Vector3Int> p_playerCells, List<Vector3Int> p_enemyCells)
	{
		PlayerCells = p_playerCells ?? new List<Vector3Int>();
		EnemyCells = p_enemyCells ?? new List<Vector3Int>();
	}
}
