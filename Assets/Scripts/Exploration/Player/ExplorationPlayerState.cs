using UnityEngine;

namespace Erelia.Exploration.Player
{
	public sealed class ExplorationPlayerState
	{
		private Vector3 worldPosition;
		private bool hasWorldPosition;
		private Vector3Int encounterLockCell;

		public Vector3 WorldPosition => worldPosition;

		public bool HasWorldPosition => hasWorldPosition;

		public void SetWorldPosition(Vector3 position)
		{
			if (!hasWorldPosition)
			{
				encounterLockCell = WorldToCell(position - Vector3.one);
			}

			worldPosition = position;
			hasWorldPosition = true;
		}

		public void SetEncounterLockCell(Vector3Int cell)
		{
			encounterLockCell = cell;
		}

		public bool IsEncounterLockedAt(Vector3Int cell)
		{
			return encounterLockCell == cell;
		}

		public void ClearEncounterLockCell(Vector3Int currentCell)
		{
			encounterLockCell = currentCell - Vector3Int.one;
		}

		private static Vector3Int WorldToCell(Vector3 worldPosition)
		{
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x),
				Mathf.FloorToInt(worldPosition.y),
				Mathf.FloorToInt(worldPosition.z));
		}
	}
}

