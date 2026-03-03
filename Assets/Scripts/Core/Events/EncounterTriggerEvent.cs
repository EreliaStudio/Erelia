using UnityEngine;

namespace Erelia.Core.Event
{
	/// <summary>
	/// Event emitted when an encounter is triggered.
	/// </summary>
	/// <remarks>
	/// Carries the encounter table used to define the encounter and the prebuilt battle board model.
	/// </remarks>
	public sealed class EncounterTriggerEvent : GenericEvent
	{
		/// <summary>
		/// Encounter table used to configure the encounter.
		/// </summary>
		public Erelia.Core.Encounter.EncounterTable EncounterTable { get; }

		/// <summary>
		/// Battle board model generated for this encounter.
		/// </summary>
		public Erelia.Battle.Board.Model BattleBoard { get; }

		/// <summary>
		/// Creates a new encounter trigger event.
		/// </summary>
		/// <param name="encounterTable">Encounter table used for this encounter.</param>
		/// <param name="battleBoard">Battle board model to use for this encounter.</param>
		public EncounterTriggerEvent(
			Erelia.Core.Encounter.EncounterTable encounterTable,
			Erelia.Battle.Board.Model battleBoard)
		{
			EncounterTable = encounterTable;
			BattleBoard = battleBoard;
		}
	}
}
