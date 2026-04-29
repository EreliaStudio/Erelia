using System;
using System.Collections.Generic;

[Serializable]
public sealed class TurnContext
{
	private readonly Dictionary<Ability, int> abilityCastCountsThisTurn = new();

	public BattleUnit ActiveUnit { get; private set; }
	public BattleSide ActiveSide => ActiveUnit != null ? ActiveUnit.Side : BattleSide.Neutral;
	public BattleAction PendingAction { get; private set; }
	public int ResolvedActionCount { get; private set; }
	public int TotalAbilityCastCountThisTurn { get; private set; }
	public IReadOnlyDictionary<Ability, int> AbilityCastCountsThisTurn => abilityCastCountsThisTurn;

	public bool HasActiveUnit => ActiveUnit != null;
	public bool HasPendingAction => PendingAction != null;

	public void Begin(BattleUnit activeUnit)
	{
		ActiveUnit = activeUnit ?? throw new ArgumentNullException(nameof(activeUnit));
		PendingAction = null;
		ResolvedActionCount = 0;
		TotalAbilityCastCountThisTurn = 0;
		abilityCastCountsThisTurn.Clear();
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

	public int RecordAbilityCast(Ability ability)
	{
		if (ability == null)
		{
			return 0;
		}

		TotalAbilityCastCountThisTurn++;
		abilityCastCountsThisTurn.TryGetValue(ability, out int count);
		count++;
		abilityCastCountsThisTurn[ability] = count;
		return count;
	}

	public void End()
	{
		ActiveUnit = null;
		PendingAction = null;
		ResolvedActionCount = 0;
		TotalAbilityCastCountThisTurn = 0;
		abilityCastCountsThisTurn.Clear();
	}
}
