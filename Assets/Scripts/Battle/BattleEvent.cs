using System;
using UnityEngine;

[Serializable]
public abstract class BattleEvent
{
	public int TurnIndex = 0;
	public Ability SourceAbility;
	public BattleUnit Caster;
	public BattleUnit Target;
}

// Replaces DamageDealtEvent + DamageTakenEvent.
// Emitted on both the caster's log and the target's log.
[Serializable]
public class DamageEvent : BattleEvent
{
	public int Amount = 0;
	public MathFormula.DamageInput.Kind DamageKind = MathFormula.DamageInput.Kind.Physical;
}

// Replaces HealthHealedEvent.
// Emitted on both the healer's log and the healed unit's log.
// IsSelf is derived from Caster == Target.
[Serializable]
public class HealEvent : BattleEvent
{
	public int Amount = 0;
}

[Serializable]
public class AbilityCastEvent : BattleEvent { }

[Serializable]
public class ShieldAppliedEvent : BattleEvent
{
	public int Amount = 0;
	public ShieldKind Kind = ShieldKind.Physical;
}

[Serializable]
public class DamageAbsorbedEvent : BattleEvent
{
	public int Amount = 0;
}

[Serializable]
public class ShieldBrokenEvent : BattleEvent
{
	public ShieldKind Kind = ShieldKind.Physical;
}

[Serializable]
public class StatusAppliedEvent : BattleEvent
{
	public Status Status;
	public int StackCount = 1;
}

[Serializable]
public class StatusRemovedEvent : BattleEvent
{
	public Status Status;
	public int StackCount = 1;
}

// Target is the defeated unit; Caster is the killer (null for environmental deaths).
[Serializable]
public class UnitDefeatedEvent : BattleEvent { }

[Serializable]
public class BattleWonEvent : BattleEvent
{
	public bool UnitSurvived = false;
}

[Serializable]
public class ResourceConsumedEvent : BattleEvent
{
	public enum ResourceKind { ActionPoints, MovementPoints }

	public ResourceKind Resource;
	public int Amount = 0;
}

[Serializable]
public class DistanceTravelledEvent : BattleEvent
{
	public int Distance = 0;
}

[Serializable]
public class TurnStartedEvent : BattleEvent
{
	public int ClosestAllyDistance = int.MaxValue;
	public int ClosestEnemyDistance = int.MaxValue;
}

[Serializable]
public class TurnEndedEvent : BattleEvent
{
	public int ClosestAllyDistance = int.MaxValue;
	public int ClosestEnemyDistance = int.MaxValue;
}

[Serializable]
public class HitSurvivedEvent : BattleEvent
{
	public int Amount = 0;
}

// Target is the teleported unit; Distance is Manhattan distance moved.
[Serializable]
public class TeleportedEvent : BattleEvent
{
	public int Distance = 0;
}

// Emitted on both the pusher's log and the pushed unit's log.
// Caster is the pusher; Target is the pushed unit.
[Serializable]
public class DisplacementEvent : BattleEvent
{
	public int Distance = 0;
	public MoveStatus.Orientation Orientation = MoveStatus.Orientation.AwayFromCaster;
}

// Emitted on both units' logs.
[Serializable]
public class SwapPositionEvent : BattleEvent { }

// Emitted when a ResourceChangeEffect modifies a resource on a target.
// Positive Value = gain, negative = loss.
[Serializable]
public class ResourceChangedEvent : BattleEvent
{
	public ResourceConsumedEvent.ResourceKind Resource;
	public int Value = 0;
}

// Emitted when a StealResourceEffect transfers a resource.
[Serializable]
public class ResourceStolenEvent : BattleEvent
{
	public StealResourceEffect.Target Resource;
	public int Amount = 0;
}

// Emitted when AdjustTurnBarTimeEffect fires (positive = faster turn, negative = slower).
[Serializable]
public class TurnBarTimeAdjustedEvent : BattleEvent
{
	public float Delta = 0f;
}

// Emitted when AdjustTurnBarDurationEffect fires.
[Serializable]
public class TurnBarDurationAdjustedEvent : BattleEvent
{
	public float Delta = 0f;
}

// Emitted when ReviveEffect restores a defeated unit.
[Serializable]
public class ReviveEvent : BattleEvent
{
	public int Amount = 0;
}
