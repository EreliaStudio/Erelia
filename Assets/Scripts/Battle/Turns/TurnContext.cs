using System;

[Serializable]
public sealed class TurnContext
{
	public BattleUnit ActiveUnit { get; private set; }
	public BattleSide ActiveSide => ActiveUnit != null ? ActiveUnit.Side : BattleSide.Neutral;
	public BattleAction PendingAction { get; private set; }
	public int ResolvedActionCount { get; private set; }

	public bool HasActiveUnit => ActiveUnit != null;
	public bool HasPendingAction => PendingAction != null;

	public void Begin(BattleUnit activeUnit)
	{
		ActiveUnit = activeUnit ?? throw new ArgumentNullException(nameof(activeUnit));
		PendingAction = null;
		ResolvedActionCount = 0;
	}

	public bool TrySetPendingAction(BattleAction action)
	{
		if (action == null || PendingAction != null || !ReferenceEquals(action.SourceUnit, ActiveUnit))
		{
			return false;
		}

		PendingAction = action;
		return true;
	}

	public BattleAction ConsumePendingAction()
	{
		BattleAction action = PendingAction;
		PendingAction = null;

		if (action != null)
		{
			ResolvedActionCount++;
		}

		return action;
	}

	public void End()
	{
		ActiveUnit = null;
		PendingAction = null;
		ResolvedActionCount = 0;
	}
}
