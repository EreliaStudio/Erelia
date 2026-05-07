internal readonly struct BattleResourceChangeResult
{
	public BattleResourceChangeResult(
		BattleUnit unit,
		BattleUnit casterUnit,
		int requestedDelta,
		int appliedDelta,
		bool crossedToDefeated)
	{
		Unit = unit;
		CasterUnit = casterUnit;
		RequestedDelta = requestedDelta;
		AppliedDelta = appliedDelta;
		CrossedToDefeated = crossedToDefeated;
	}

	public BattleUnit Unit { get; }
	public BattleUnit CasterUnit { get; }
	public int RequestedDelta { get; }
	public int AppliedDelta { get; }
	public bool CrossedToDefeated { get; }
	public bool Changed => AppliedDelta != 0;
	public bool IsLoss => AppliedDelta < 0;
	public bool IsGain => AppliedDelta > 0;
	public int LossAmount => AppliedDelta < 0 ? -AppliedDelta : 0;
	public int GainAmount => AppliedDelta > 0 ? AppliedDelta : 0;
}
