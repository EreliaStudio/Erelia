using System;
using UnityEngine;

[Serializable]
public sealed class BattleAbilityExecutionContext
{
	public BattleContext BattleContext { get; set; }
 	public Ability Ability { get; set; }
	public BattleObject SourceObject { get; set; }
	public BattleObject TargetObject { get; set; }
	public Vector3Int AnchorCell { get; set; }
	public Vector3Int AffectedCell { get; set; }

	public BattleUnit SourceUnit => SourceObject as BattleUnit;
	public BattleUnit TargetUnit => TargetObject as BattleUnit;
	public bool HasTargetObject => TargetObject != null;
}
