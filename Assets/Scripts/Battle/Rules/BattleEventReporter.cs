public static class BattleEventReporter
{
	public static void Emit(BattleEvent p_featEvent)
	{
		if (p_featEvent == null)
		{
			return;
		}

		EventCenter.EmitBattleEventOccurred(p_featEvent);
	}
}
