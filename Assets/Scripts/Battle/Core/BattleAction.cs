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
}

public sealed class MoveAction : BattleAction
{
	public MoveAction(BattleUnit p_sourceUnit, Vector3Int p_destination) : base(p_sourceUnit)
	{
		Destination = p_destination;
	}

	public Vector3Int Destination { get; }
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
}

public sealed class EndTurnAction : BattleAction
{
	public EndTurnAction(BattleUnit p_sourceUnit) : base(p_sourceUnit)
	{
	}
}
