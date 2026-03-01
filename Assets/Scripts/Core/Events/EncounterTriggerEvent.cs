using UnityEngine;

namespace Erelia.Core.Event
{
	public sealed class EncounterTriggerEvent : GenericEvent
	{
		public Erelia.Core.Encounter.EncounterTable EncounterTable { get; }
		public Erelia.Battle.Board.Model BattleBoard { get; }

		public EncounterTriggerEvent(
			Erelia.Core.Encounter.EncounterTable encounterTable,
			Erelia.Battle.Board.Model battleBoard)
		{
			EncounterTable = encounterTable;
			BattleBoard = battleBoard;
		}
	}
}
