using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player
{
	[Serializable]
	public class Service
	{			
		public event Action<World.Chunk.Model.Coordinates> PlayerChunkCoordinateChanged;
		[SerializeField] private GameObject playerObject = null;
		private Vector3Int lastBushGrid = new Vector3Int(0, 0, 0);
		private World.Chunk.Model.Coordinates currentChunk = new World.Chunk.Model.Coordinates(0, 0, 0);
		private Battle.EncounterTable.Model.Data currentEncounterTable = null;

		public void Init()
		{
			
		}

		public void NotifyChunkCoordinateChanged()
		{
			if (playerObject == null)
			{
				Debug.LogError("Player.Service: playerObject is not assigned.");
				return;
			}

			World.Chunk.Model.Coordinates coord = World.Chunk.Model.Coordinates.FromWorld(playerObject.transform.position);
			currentChunk = coord;
			currentEncounterTable = Utils.ServiceLocator.Instance.EncounterService.GetEncounterTable(coord);
			PlayerChunkCoordinateChanged?.Invoke(coord);
		}

		public void NotifyPlayerWalkingInBush()
		{
			if (playerObject == null)
			{
				Debug.LogError("Player.Service: playerObject is not assigned.");
				return;
			}

			Vector3Int grid = Vector3Int.FloorToInt(playerObject.transform.position);
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
