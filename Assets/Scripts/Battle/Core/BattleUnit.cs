using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleUnit : BattleObject
{
	public Vector3Int BoardPosition;
	public bool HasBoardPosition;
	public bool HasLeftBattle;

	public BattleUnit(CreatureUnit p_sourceUnit, BattleSide p_side)
	{
		SourceUnit = p_sourceUnit ?? throw new ArgumentNullException(nameof(p_sourceUnit));
		Side = p_side;

		BattleAttributes = new BattleAttributes(SourceUnit.Attributes);

		foreach (Status status in SourceUnit.PermanentPassives)
		{
			if (status == null)
			{
				continue;
			}

			Statuses.Add(status, 1, new Duration { Type = Duration.Kind.Infinite }, true);
		}
	}

	public CreatureUnit SourceUnit { get; }
	public BattleAttributes BattleAttributes { get; }
	public BattleStatuses Statuses { get; } = new();

	public IReadOnlyList<Ability> Abilities => SourceUnit.GetAbilities();
	public bool IsDefeated => BattleAttributes.Health.Current <= 0;
	public bool IsActiveInBattle => !IsDefeated && !HasLeftBattle;
	public bool IsTurnReady => BattleAttributes.TurnBar.Current >= BattleAttributes.TurnBar.Max;

	public event Action<BattleUnit, Vector3Int?> PositionChanged;

	public void SetBoardPosition(Vector3Int p_position)
	{
		BoardPosition = p_position;
		HasBoardPosition = true;
		PositionChanged?.Invoke(this, p_position);
	}

	public void ClearBoardPosition()
	{
		if (!HasBoardPosition)
		{
			return;
		}

		HasBoardPosition = false;
		PositionChanged?.Invoke(this, null);
	}

	public void MarkLeftBattle()
	{
		HasLeftBattle = true;
	}

	public virtual void ResetBattleRuntimeState()
	{
		HasLeftBattle = false;
		ClearBoardPosition();
		BattleAttributes.Setup(SourceUnit.Attributes);
		BattleAttributes.ClearShields();
	}
}
