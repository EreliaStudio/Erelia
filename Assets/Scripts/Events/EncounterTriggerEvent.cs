namespace Erelia.Event
{
	public sealed class EncounterTriggerEvent : GenericEvent
	{
		public Erelia.Encounter.EncounterTable EncounterTable { get; }

		public EncounterTriggerEvent(Erelia.Encounter.EncounterTable encounterTable)
		{
			EncounterTable = encounterTable;
		}
	}
}
