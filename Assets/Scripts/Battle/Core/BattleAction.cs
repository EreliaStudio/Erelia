using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleAction
{
	protected BattleAction(BattleUnit p_sourceUnit)
	{
		SourceUnit = p_sourceUnit ?? throw new ArgumentNullException(nameof(p_sourceUnit));
	}

	public BattleUnit SourceUnit { get; }
	public virtual int ActionPointCost => 0;
	public virtual int MovementPointCost => 0;
}

public sealed class MoveAction : BattleAction
{
	public MoveAction(BattleUnit p_sourceUnit, Vector3Int p_destination, int p_distance) : base(p_sourceUnit)
	{
		Destination = p_destination;
		Distance = Math.Max(0, p_distance);
	}

	public Vector3Int Destination { get; }
	public int Distance { get; }
	public override int MovementPointCost => Distance;
}

public sealed class AbilityAction : BattleAction
{
	public AbilityAction(BattleUnit p_sourceUnit, Ability p_ability, IReadOnlyList<Vector3Int> p_targetCells) : base(p_sourceUnit)
	{
		Ability = p_ability ?? throw new ArgumentNullException(nameof(p_ability));
		TargetCells = p_targetCells ?? Array.Empty<Vector3Int>();
	}

	public Ability Ability { get; }
	public IReadOnlyList<Vector3Int> TargetCells { get; }
	public override int ActionPointCost => Math.Max(0, Ability?.Cost?.Ability ?? 0);
	public override int MovementPointCost => Math.Max(0, Ability?.Cost?.Movement ?? 0);
}

public sealed class EndTurnAction : BattleAction
{
	public EndTurnAction(BattleUnit p_sourceUnit) : base(p_sourceUnit)
	{
	}
}
