using System;
using UnityEngine;

[Serializable]
public sealed class BattleAbilityExecutionContext
{
	public BattleContext BattleContext { get; init; }
	public Ability Ability { get; init; }
	public BattleObject SourceObject { get; init; }
	public BattleObject TargetObject { get; init; }
	public Vector3Int AnchorCell { get; init; }
	public Vector3Int AffectedCell { get; init; }

	public BattleUnit SourceUnit => SourceObject as BattleUnit;
	public BattleUnit TargetUnit => TargetObject as BattleUnit;
	public bool HasTargetObject => TargetObject != null;
}
