using System.Collections.Generic;

public sealed class BattleLogService
{
	private readonly List<BattleEvent> battleLog = new();

	public IReadOnlyList<BattleEvent> BattleLog => battleLog;

	public void Initialize()
	{
		EventCenter.BattleStarted += OnBattleStarted;
		EventCenter.BattleEventOccurred += OnBattleEventOccurred;
	}

	public void Shutdown()
	{
		EventCenter.BattleStarted -= OnBattleStarted;
		EventCenter.BattleEventOccurred -= OnBattleEventOccurred;
		battleLog.Clear();
	}

	private void OnBattleStarted(BattleContext p_battleContext)
	{
		battleLog.Clear();
	}

	private void OnBattleEventOccurred(BattleEvent p_event)
	{
		if (p_event != null)
		{
			battleLog.Add(p_event);
		}
	}
}
