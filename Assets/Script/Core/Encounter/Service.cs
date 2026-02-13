using UnityEngine;
using System;
using Utils;

namespace Core.Encounter
{
	[Serializable]
	public class Service
	{
		[SerializeField] private Core.Encounter.Table.Model.Data defaultEncounterTable = null;

		public Service(Core.Encounter.Table.Model.Data defaultEncounterTable)
		{
			this.defaultEncounterTable = defaultEncounterTable;
		}

		public Core.Encounter.Table.Model.Data GetEncounterTable(Exploration.World.Chunk.Model.Coordinates coord)
		{
			return defaultEncounterTable;
		} 

		public bool TryStartEncounter()
		{
			Core.Player.Model.Data playerData = ServiceLocator.Instance.PlayerService.PlayerData;
			if (playerData == null)
			{
				return false;
			}

			Exploration.World.Chunk.Model.Coordinates coord = playerData.ChunkCoordinates;
			Core.Encounter.Table.Model.Data encounterTable = GetEncounterTable(coord);

			if (encounterTable == null)
			{
				return false;
			}

			float roll = UnityEngine.Random.value;
			if (roll > encounterTable.FightChance)
			{
				return false;
			}

			Debug.Log($"Encounter triggered (roll {roll:0.000} <= {encounterTable.FightChance:0.000}).");

			ServiceLocator.Instance.BattleBoardService.SetData(
				ServiceLocator.Instance.WorldService.ExtrudeCells(
					new Vector2Int(playerData.CellPosition.x, playerData.CellPosition.y),
					encounterTable.BoardArea));

			ServiceLocator.Instance.SceneLoader.LoadBattleScene();
			return true;
		}
	}
}
