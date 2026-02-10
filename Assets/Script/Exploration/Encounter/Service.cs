using UnityEngine;
using System;

namespace Exploration.Encounter
{
	[Serializable]
	public class Service
	{
		[SerializeField] private Battle.EncounterTable.Model.Data defaultEncounterTable = null;

		public void Init()
		{
		}

		public Battle.EncounterTable.Model.Data GetEncounterTable(World.Chunk.Model.Coordinates coord)
		{
			return defaultEncounterTable;
		}
	}
}
