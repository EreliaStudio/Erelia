using UnityEngine;
using System;
using Utils;

namespace Exploration.Encounter
{
	[Serializable]
	public class Service
	{
		[SerializeField] private Battle.EncounterTable.Model.Data defaultEncounterTable = null;

		public Service(Battle.EncounterTable.Model.Data defaultEncounterTable)
		{
			this.defaultEncounterTable = defaultEncounterTable;
		}

		public Battle.EncounterTable.Model.Data GetEncounterTable(World.Chunk.Model.Coordinates coord)
		{
			return defaultEncounterTable;
		}

		public bool TryStartEncounter()
		{
			Player.Model.Data playerData = ServiceLocator.Instance.PlayerService.PlayerData;
			if (playerData == null)
			{
				return false;
			}

			World.Chunk.Model.Coordinates coord = playerData.ChunkCoordinates;
			Battle.EncounterTable.Model.Data encounterTable = GetEncounterTable(coord);

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
