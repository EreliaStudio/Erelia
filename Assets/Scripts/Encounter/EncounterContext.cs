namespace Erelia.Encounter
{
	public static class EncounterContext
	{
		public static Erelia.Event.EncounterTriggerEvent LastEncounter { get; private set; }
		public static bool BattleReady { get; private set; }

		public static void SetEncounter(Erelia.Event.EncounterTriggerEvent evt)
		{
			LastEncounter = evt;
			BattleReady = false;
		}

		public static void SetBattleReady()
		{
			BattleReady = true;
		}

		public static void Clear()
		{
			LastEncounter = null;
			BattleReady = false;
		}
	}
}
