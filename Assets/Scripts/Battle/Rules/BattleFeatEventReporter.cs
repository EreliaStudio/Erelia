public static class BattleFeatEventReporter
{
	public static void Emit(BattleUnit p_unit, BattleEvent p_featEvent)
	{
		if (p_unit == null || p_featEvent == null)
		{
			return;
		}

		EventCenter.EmitBattleEventOccurred(p_unit, p_featEvent);
	}

	// Emits the same event on both the caster's and target's logs.
	// The Caster and Target fields must already be set on the event before calling this.
	public static void EmitBoth(BattleUnit p_caster, BattleUnit p_target, BattleEvent p_featEvent)
	{
		if (p_featEvent == null)
		{
			return;
		}

		if (p_caster != null)
		{
			Emit(p_caster, p_featEvent);
		}

		if (p_target != null && p_target != p_caster)
		{
			Emit(p_target, p_featEvent);
		}
	}
}
