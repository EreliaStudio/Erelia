using System;

[Serializable]
public sealed class WildBattleUnit : BattleUnit
{
	private readonly FeatRequirement.EventBase[] singleEventBuffer = new FeatRequirement.EventBase[1];

	public TamingProgress TamingProgress { get; }
	public bool IsTamed => TamingProgress?.IsImpressed ?? false;

	public WildBattleUnit(CreatureUnit p_sourceUnit, BattleSide p_side, TamingProfile p_profile)
		: base(p_sourceUnit, p_side)
	{
		TamingProgress = new TamingProgress(this, p_profile);
	}

	public void EvaluateTamingEvent(FeatRequirement.EventBase p_event)
	{
		if (p_event == null)
		{
			return;
		}

		singleEventBuffer[0] = p_event;
		TamingProgress.EvaluateEvents(singleEventBuffer);
	}

	public void MarkUntamable()
	{
		TamingProgress?.MarkFailed();
	}

	public override void ResetBattleRuntimeState()
	{
		base.ResetBattleRuntimeState();
		TamingProgress?.Reset();
	}
}
