using UnityEngine;
using System;
using Utils;

namespace Player
{
	[Serializable]
	public class Service
	{
		public event Action<World.Chunk.Model.Coordinates> PlayerChunkCoordinateChanged;
		private readonly Player.Model.Data playerData = new Player.Model.Data();
		private Vector3Int lastBushCell = new Vector3Int();
		private World.Chunk.Model.Coordinates lastChunkCoordinates = new World.Chunk.Model.Coordinates();

		public Player.Model.Data PlayerData => playerData;

		public void Init()
		{
			lastBushCell = new Vector3Int(playerData.CellPosition.x - 1, playerData.CellPosition.y - 1, playerData.CellPosition.z - 1);
			lastChunkCoordinates = new World.Chunk.Model.Coordinates(playerData.ChunkCoordinates.X - 1, playerData.ChunkCoordinates.Y - 1, playerData.ChunkCoordinates.Z - 1);
		}

		public void UpdatePlayerPosition(Vector3 worldPosition)
		{
			playerData.UpdateFromWorld(worldPosition);

			if (!lastChunkCoordinates.Equals(playerData.ChunkCoordinates))
			{
				lastChunkCoordinates = playerData.ChunkCoordinates;
				PlayerChunkCoordinateChanged?.Invoke(playerData.ChunkCoordinates);
			}
		}

		public void NotifyPlayerWalkingInBush()
		{
			if (playerData.CellPosition == lastBushCell)
			{
				return;
			}

			lastBushCell = playerData.CellPosition;

			ServiceLocator.Instance.EncounterService.TryStartEncounter();
		}
	}
}
