using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player
{
	[Serializable]
	public class Service
	{			
		public event Action<World.Chunk.Model.Coordinates> PlayerChunkCoordinateChanged;
		private Vector3Int lastBushGrid = new Vector3Int(0, 0, 0);
		private World.Chunk.Model.Coordinates currentChunk = new World.Chunk.Model.Coordinates(0, 0, 0);
		private Battle.EncounterTable.Model.Data currentEncounterTable = null;

		public void Init()
		{
			
		}

		public void NotifyChunkCoordinateChanged(World.Chunk.Model.Coordinates coord)
		{
			currentChunk = coord;
			currentEncounterTable = Utils.ServiceLocator.Instance.EncounterService.GetEncounterTable(coord);
			PlayerChunkCoordinateChanged?.Invoke(coord);
		}

		public void NotifyPlayerWalkingInBush(Vector3 worldPosition)
		{
			Vector3Int grid = Vector3Int.FloorToInt(worldPosition);
			if (grid == lastBushGrid || currentEncounterTable == null)
			{
				return;
			}
			lastBushGrid = grid;

			float roll = UnityEngine.Random.value;
			if (roll <= currentEncounterTable.FightChance)
			{
				Debug.Log($"Encounter triggered (roll {roll:0.000} <= {currentEncounterTable.FightChance:0.000}).");
			}
		}
	}
}
