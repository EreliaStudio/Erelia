public static class BattleFeatEventReporter
{
	public static void Emit(BattleUnit p_unit, FeatRequirement.EventBase p_featEvent)
	{
		if (p_unit == null || p_featEvent == null)
		{
			return;
		}

		EventCenter.EmitBattleFeatEventOccurred(p_unit, p_featEvent);
	}
}
