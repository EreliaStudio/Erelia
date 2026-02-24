using UnityEngine;

namespace Erelia.Event
{
	public sealed class EncounterTriggerEvent : GenericEvent
	{
		public Erelia.Encounter.EncounterTable EncounterTable { get; }
		public Erelia.Battle.Board.Model BattleBoard { get; }

		public EncounterTriggerEvent(
			Erelia.Encounter.EncounterTable encounterTable,
			Erelia.Battle.Board.Model battleBoard)
		{
			EncounterTable = encounterTable;
			BattleBoard = battleBoard;
		}
	}
}
