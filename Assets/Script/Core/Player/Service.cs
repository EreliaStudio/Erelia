using UnityEngine;
using System;
using Utils;

namespace Core.Player
{
	[Serializable]
	public class Service
	{
		public event Action<Exploration.World.Chunk.Model.Coordinates> PlayerChunkCoordinateChanged;
		private readonly Model.Data playerData = new Model.Data();
		private Core.Creature.Model.Team team = null;
		private Vector3Int lastBushCell = new Vector3Int();
		private Exploration.World.Chunk.Model.Coordinates lastChunkCoordinates = new Exploration.World.Chunk.Model.Coordinates();

		public Model.Data PlayerData => playerData;
		public Core.Creature.Model.Team Team => team;

		public Service(Core.Creature.Model.Team team)
		{
			lastBushCell = new Vector3Int(playerData.CellPosition.x - 1, playerData.CellPosition.y - 1, playerData.CellPosition.z - 1);
			lastChunkCoordinates = new Exploration.World.Chunk.Model.Coordinates(playerData.ChunkCoordinates.X - 1, playerData.ChunkCoordinates.Y - 1, playerData.ChunkCoordinates.Z - 1);
			SetTeam(team);
		}

		public void SetTeam(Core.Creature.Model.Team team)
		{
			this.team = team;
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
