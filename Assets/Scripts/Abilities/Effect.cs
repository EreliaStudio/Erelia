using System;
using System.Collections.Generic;
using UnityEngine;
using static EffectUtility;

[Serializable]
public abstract class Effect
{
	public abstract void Apply(BattleAbilityExecutionContext context);
}

[Serializable]
public class ApplyStatusEffect : Effect
{
	public Status Status;
	public Duration Duration = new Duration();
	public int StackCount = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Status == null || StackCount <= 0)
		{
			return;
		}

		context.TargetUnit.Statuses.Add(Status, StackCount, Duration.Clone(Duration));
		BattleEventReporter.Emit(
			new StatusAppliedEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Status = Status,
				StackCount = StackCount,
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class RemoveStatusEffect : Effect
{
	public Status Status;
	public int StackCount = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Status == null)
		{
			return;
		}

		context.TargetUnit.Statuses.Remove(Status, Math.Max(1, StackCount));
		BattleEventReporter.Emit(
			new StatusRemovedEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Status = Status,
				StackCount = Math.Max(1, StackCount),
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class ReviveEffect : Effect
{
	public int HealthRestored = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || !context.TargetUnit.IsDefeated)
		{
			return;
		}

		int restoredHealth = MathFormula.ComputeHealing(
			context.SourceUnit?.SourceUnit?.Attributes,
			new MathFormula.HealingInput
			{
				BaseHealing = Math.Max(1, HealthRestored)
			});

		BattleResourceRules.ChangeHealth(
			context.BattleContext,
			context.TargetUnit,
			context.SourceUnit,
			Math.Max(1, restoredHealth));

		BattleEventReporter.Emit(
			new ReviveEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Amount = Math.Max(1, restoredHealth),
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class CleanseEffect : Effect
{
	public List<string> TagsToCleanse = new List<string>();

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || TagsToCleanse == null || TagsToCleanse.Count == 0)
		{
			return;
		}

		context.TargetUnit.Statuses.Remove(TagsToCleanse);
	}
}

[Serializable]
public class ResourceChangeEffect : Effect
{
	public enum Target
	{
		ActionPoint,
		MovementPoint,
		Range
	};

	public Target ResourceTargeted = Target.ActionPoint;
	public int Value = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Value == 0)
		{
			return;
		}

		switch (ResourceTargeted)
		{
			case Target.ActionPoint:
				BattleResourceRules.ChangeActionPoints(
					context.BattleContext,
					context.TargetUnit,
					context.SourceUnit,
					Value);
				BattleEventReporter.Emit(new ResourceChangedEvent
					{
						Caster = context.SourceUnit,
						Target = context.TargetUnit,
						Resource = ResourceConsumedEvent.ResourceKind.ActionPoints,
						Value = Value,
						SourceAbility = context.Ability
					});
				break;

			case Target.MovementPoint:
				BattleResourceRules.ChangeMovementPoints(
					context.BattleContext,
					context.TargetUnit,
					context.SourceUnit,
					Value);
				BattleEventReporter.Emit(new ResourceChangedEvent
					{
						Caster = context.SourceUnit,
						Target = context.TargetUnit,
						Resource = ResourceConsumedEvent.ResourceKind.MovementPoints,
						Value = Value,
						SourceAbility = context.Ability
					});
				break;

			case Target.Range:
				context.TargetUnit.BattleAttributes.BonusRange.Set(Math.Max(0, context.TargetUnit.BattleAttributes.BonusRange.Value + Value));
				break;
		}
	}
}

[Serializable]
public class MoveStatus : Effect
{
	public enum Orientation
	{
		TowardCaster,
		AwayFromCaster
	};

	public Orientation ForceOrientation = Orientation.AwayFromCaster;
	public int Force = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null ||
			context.SourceObject == null ||
			context.TargetUnit == null ||
			Force <= 0)
		{
			return;
		}

		if (!context.BattleContext.Board.Runtime.TryGetPosition(context.SourceObject, out Vector3Int casterPosition))
		{
			return;
		}

		if (!context.BattleContext.Board.Runtime.TryGetPosition(context.TargetUnit, out Vector3Int targetPosition))
		{
			return;
		}

		Vector3Int step = ComputeDisplacementStep(casterPosition, targetPosition, ForceOrientation);
		if (step == Vector3Int.zero)
		{
			return;
		}

		Vector3Int currentPosition = targetPosition;
		int distanceMoved = 0;
		for (int index = 0; index < Force; index++)
		{
			Vector3Int nextPosition = currentPosition + step;
			if (!context.BattleContext.TryMoveUnit(context.TargetUnit, nextPosition))
			{
				break;
			}

			currentPosition = nextPosition;
			distanceMoved++;
		}

		if (distanceMoved > 0)
		{
			BattleEventReporter.Emit(new DisplacementEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					Distance = distanceMoved,
					Orientation = ForceOrientation,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class SwapPositionEffect : Effect
{
	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null ||
			context.TargetUnit == null ||
			context.AffectedCell == context.AnchorCell)
		{
			return;
		}

		IReadOnlyList<BattleObject> anchorObjects = BattleTargetingRules.GetObjectsAtCell(context.BattleContext, context.AnchorCell);
		BattleUnit anchorUnit = anchorObjects.Count > 0 ? anchorObjects[0] as BattleUnit : null;
		if (anchorUnit == null)
		{
			return;
		}

		bool swapped = context.BattleContext.TrySwapUnits(anchorUnit, context.TargetUnit);
		if (swapped)
		{
			BattleEventReporter.Emit(new SwapPositionEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class SwapPositionWithCasterEffect : Effect
{
	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null ||
			context.SourceUnit == null ||
			context.TargetUnit == null)
		{
			return;
		}

		bool swapped = context.BattleContext.TrySwapUnits(context.SourceUnit, context.TargetUnit);
		if (swapped)
		{
			BattleEventReporter.Emit(new SwapPositionEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class TeleportEffect : Effect
{
	public Vector3Int Destination = Vector3Int.zero;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null || context.TargetUnit == null)
		{
			return;
		}

		context.BattleContext.Board.Runtime.TryGetPosition(context.TargetUnit, out Vector3Int fromPosition);
		bool teleported = context.BattleContext.TryPlaceUnit(context.TargetUnit, Destination);
		if (teleported)
		{
			int distance = Math.Abs(Destination.x - fromPosition.x) + Math.Abs(Destination.z - fromPosition.z);
			BattleEventReporter.Emit(new TeleportedEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					Distance = distance,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class TeleportSelfEffect : Effect
{
	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null || context.SourceUnit == null)
		{
			return;
		}

		// Only execute once, at the anchor cell, to avoid re-teleporting for each AOE cell.
		if (context.AffectedCell != context.AnchorCell)
		{
			return;
		}

		context.BattleContext.Board.Runtime.TryGetPosition(context.SourceUnit, out Vector3Int fromPosition);
		bool teleported = context.BattleContext.TryPlaceUnit(context.SourceUnit, context.AnchorCell);
		if (teleported)
		{
			int distance = Math.Abs(context.AnchorCell.x - fromPosition.x) + Math.Abs(context.AnchorCell.z - fromPosition.z);
			BattleEventReporter.Emit(new TeleportedEvent
				{
					Caster = context.SourceUnit,
					Target = context.SourceUnit,
					Distance = distance,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class StealResourceEffect : Effect
{
	public enum Target
	{
		Health,
		ActionPoint,
		MovementPoint,
		Range,
		Stamina
	};

	public Target ResourceTargeted = Target.ActionPoint;
	public int Value = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.SourceUnit == null || context.TargetUnit == null || Value <= 0)
		{
			return;
		}

		int stolenAmount = 0;
		switch (ResourceTargeted)
		{
			case Target.Health:
				stolenAmount = StealHealth(context.BattleContext, context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.ActionPoint:
				stolenAmount = StealActionPoints(context.BattleContext, context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.MovementPoint:
				stolenAmount = StealMovementPoints(context.BattleContext, context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.Range:
				stolenAmount = StealRange(context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.Stamina:
				stolenAmount = StealTurnBarTime(context.SourceUnit, context.TargetUnit, Value);
				break;
		}

		if (stolenAmount > 0)
		{
			BattleEventReporter.Emit(new ResourceStolenEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					Resource = ResourceTargeted,
					Amount = stolenAmount,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class ConsumeStatus : Effect
{
	public Status Status;
	public int NbOfStackConsumed = 1;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Status == null)
		{
			return;
		}

		context.TargetUnit.Statuses.Remove(Status, Math.Max(1, NbOfStackConsumed));
	}
}

[Serializable]
public class AdjustTurnBarTimeEffect : Effect
{
	public float Delta = 1f;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Mathf.Approximately(Delta, 0f))
		{
			return;
		}

		float adjustedDelta = MathFormula.ComputeTurnBarTimeDelta(
			Delta,
			context.TargetUnit.SourceUnit?.Attributes);

		if (adjustedDelta > 0f)
		{
			context.TargetUnit.BattleAttributes.TurnBar.Increase(adjustedDelta);
		}
		else
		{
			context.TargetUnit.BattleAttributes.TurnBar.Decrease(-adjustedDelta);
		}

		BattleEventReporter.Emit(
			new TurnBarTimeAdjustedEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Delta = adjustedDelta,
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class AdjustTurnBarDurationEffect : Effect
{
	public float Delta = 1f;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Mathf.Approximately(Delta, 0f))
		{
			return;
		}

		float adjustedDelta = MathFormula.ComputeTurnBarDurationDelta(
			Delta,
			context.TargetUnit.SourceUnit?.Attributes);

		context.TargetUnit.BattleAttributes.TurnBar.SetMax(Math.Max(MathFormula.MinimumTurnBarDuration, context.TargetUnit.BattleAttributes.TurnBar.Max + adjustedDelta));

		BattleEventReporter.Emit(
			new TurnBarDurationAdjustedEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Delta = adjustedDelta,
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class DamageTargetEffect : Effect
{
	public MathFormula.DamageInput Input = new MathFormula.DamageInput
	{
		BaseDamage = 1,
		DamageKind = MathFormula.DamageInput.Kind.Physical,
		AttackRatio = 1f,
		MagicRatio = 0f
	};

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Input.BaseDamage <= 0)
		{
			return;
		}

		Attributes casterAttributes = context.SourceUnit?.SourceUnit?.Attributes;
		Attributes targetAttributes = context.TargetUnit.SourceUnit?.Attributes;
		int computedDamage = MathFormula.ComputeDamage(casterAttributes, targetAttributes, Input);

		if (computedDamage <= 0)
		{
			return;
		}

		// Shields absorb their matching damage kind before HP is touched.
		BattleShieldAbsorptionResult shieldAbsorption =
			context.TargetUnit.BattleAttributes.AbsorbDamage(Input.DamageKind, computedDamage);
		int absorbedByShield = shieldAbsorption.AmountAbsorbed;
		int damageToHP = computedDamage - absorbedByShield;

		int appliedToHP = 0;
		if (damageToHP > 0)
		{
			BattleResourceChangeResult healthChange = BattleResourceRules.ChangeHealth(
				context.BattleContext,
				context.TargetUnit,
				context.SourceUnit,
				-damageToHP);
			appliedToHP = healthChange.LossAmount;
		}

		int totalApplied = absorbedByShield + appliedToHP;
		if (totalApplied <= 0)
		{
			return;
		}

		// Vampirism only applies to HP damage, not shield absorption.
		int selfHealing = MathFormula.ComputeVampirismHealing(casterAttributes, Input.DamageKind, appliedToHP);
		if (selfHealing > 0 && context.SourceUnit != null)
		{
			BattleResourceRules.ChangeHealth(
				context.BattleContext,
				context.SourceUnit,
				context.SourceUnit,
				selfHealing);
		}

		BattleEventReporter.Emit(
			new DamageEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Amount = totalApplied,
				DamageKind = Input.DamageKind,
				SourceAbility = context.Ability
			});

		if (absorbedByShield > 0)
		{
			BattleEventReporter.Emit(new DamageAbsorbedEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					Amount = absorbedByShield,
					SourceAbility = context.Ability
				});
		}

		IReadOnlyList<ShieldKind> brokenShieldKinds = shieldAbsorption.BrokenShieldKinds;
		for (int index = 0; index < brokenShieldKinds.Count; index++)
		{
			BattleEventReporter.Emit(new ShieldBrokenEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					Kind = brokenShieldKinds[index],
					SourceAbility = context.Ability
				});
		}

		if (!context.TargetUnit.IsDefeated)
		{
			BattleEventReporter.Emit(new HitSurvivedEvent
				{
					Caster = context.SourceUnit,
					Target = context.TargetUnit,
					Amount = totalApplied,
					SourceAbility = context.Ability
				});
		}
	}
}

[Serializable]
public class ApplyShieldEffect : Effect
{
	public ShieldKind Kind = ShieldKind.Physical;
	public int Amount = 10;
	public int DurationInTurns = 1; // -1 = infinite

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Amount <= 0)
		{
			return;
		}

		context.TargetUnit.BattleAttributes.AddShield(Kind, Amount, DurationInTurns);
		BattleEventReporter.Emit(
			new ShieldAppliedEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Amount = Amount,
				Kind = Kind,
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class HealTargetEffect : Effect
{
	public MathFormula.HealingInput Input = new MathFormula.HealingInput
	{
		BaseHealing = 1,
		AttackRatio = 0f,
		MagicRatio = 0f
	};

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.TargetUnit == null || Input.BaseHealing <= 0)
		{
			return;
		}

		int computedHealing = MathFormula.ComputeHealing(
			context.SourceUnit?.SourceUnit?.Attributes,
			Input);

		if (computedHealing <= 0)
		{
			return;
		}

		BattleResourceRules.ChangeHealth(
			context.BattleContext,
			context.TargetUnit,
			context.SourceUnit,
			computedHealing);
		BattleEventReporter.Emit(
			new HealEvent
			{
				Caster = context.SourceUnit,
				Target = context.TargetUnit,
				Amount = computedHealing,
				SourceAbility = context.Ability
			});
	}
}

[Serializable]
public class PlaceInteractiveObjectEffect : Effect
{
	public InteractionObject InteractionObject;
	public Duration Duration = new Duration();

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null || InteractionObject == null)
		{
			return;
		}

		BattleInteractiveObject interactiveObject = new BattleInteractiveObject
		{
			Side = context.SourceObject != null ? context.SourceObject.Side : BattleSide.Neutral,
			InteractionObject = InteractionObject,
			Tags = InteractionObject.Tags != null ? new List<string>(InteractionObject.Tags) : new List<string>(),
			RemainingDuration = Duration.Clone(Duration)
		};

		context.BattleContext.Board.Runtime.TryAddInteractiveObject(interactiveObject, context.AffectedCell);
	}
}

[Serializable]
public class RemoveInteractiveObjectEffect : Effect
{
	public List<string> Tags = new List<string>();

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null || Tags == null || Tags.Count == 0)
		{
			return;
		}

		context.BattleContext.Board.Runtime.RemoveInteractiveObjectsByTags(context.AffectedCell, Tags);
	}
}

internal static class EffectUtility
{
	public static bool TryResolveAnchorPosition(BattleAbilityExecutionContext context, out Vector3Int position)
	{
		position = default;

		if (context == null)
		{
			return false;
		}

		if (context.BattleContext?.Board != null)
		{
			if (context.BattleContext.Board.IsInside(context.AffectedCell))
			{
				position = context.AffectedCell;
				return true;
			}

			if (context.BattleContext.Board.IsInside(context.AnchorCell))
			{
				position = context.AnchorCell;
				return true;
			}
		}

		if (context.TargetObject != null && context.BattleContext != null && context.BattleContext.Board.Runtime.TryGetPosition(context.TargetObject, out position))
		{
			return true;
		}

		if (context.SourceObject != null && context.BattleContext != null && context.BattleContext.Board.Runtime.TryGetPosition(context.SourceObject, out position))
		{
			return true;
		}

		position = default;
		return false;
	}

	public static Vector3Int ComputeDisplacementStep(Vector3Int casterPosition, Vector3Int targetPosition, MoveStatus.Orientation orientation)
	{
		Vector3Int delta = orientation == MoveStatus.Orientation.TowardCaster
			? casterPosition - targetPosition
			: targetPosition - casterPosition;

		if (delta == Vector3Int.zero)
		{
			return Vector3Int.zero;
		}

		int stepX = Math.Sign(delta.x);
		int stepZ = Math.Sign(delta.z);

		if (Math.Abs(delta.x) > Math.Abs(delta.z))
		{
			stepZ = 0;
		}
		else if (Math.Abs(delta.z) > Math.Abs(delta.x))
		{
			stepX = 0;
		}

		return new Vector3Int(stepX, 0, stepZ);
	}

	public static int StealHealth(
		BattleContext battleContext,
		BattleUnit casterUnit,
		BattleUnit targetUnit,
		int value)
	{
		BattleResourceChangeResult targetHealthChange = BattleResourceRules.ChangeHealth(battleContext, targetUnit, casterUnit, -value);
		int stolenHealth = targetHealthChange.LossAmount;
		if (stolenHealth <= 0)
		{
			return 0;
		}

		BattleResourceRules.ChangeHealth(battleContext, casterUnit, casterUnit, stolenHealth);
		return stolenHealth;
	}

	public static int StealActionPoints(BattleContext battleContext, BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		BattleResourceChangeResult targetPointChange = BattleResourceRules.ChangeActionPoints(battleContext, targetUnit, casterUnit, -value);
		int stolenPoints = targetPointChange.LossAmount;
		if (stolenPoints <= 0)
		{
			return 0;
		}

		BattleResourceRules.ChangeActionPoints(battleContext, casterUnit, casterUnit, stolenPoints);
		return stolenPoints;
	}

	public static int StealMovementPoints(BattleContext battleContext, BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		BattleResourceChangeResult targetPointChange = BattleResourceRules.ChangeMovementPoints(battleContext, targetUnit, casterUnit, -value);
		int stolenPoints = targetPointChange.LossAmount;
		if (stolenPoints <= 0)
		{
			return 0;
		}

		BattleResourceRules.ChangeMovementPoints(battleContext, casterUnit, casterUnit, stolenPoints);
		return stolenPoints;
	}

	public static int StealRange(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		int stolenRange = Math.Min(value, targetUnit.BattleAttributes.BonusRange.Value);
		if (stolenRange <= 0)
		{
			return 0;
		}

		targetUnit.BattleAttributes.BonusRange.Set(Math.Max(0, targetUnit.BattleAttributes.BonusRange.Value - stolenRange));
		casterUnit.BattleAttributes.BonusRange.Set(Math.Max(0, casterUnit.BattleAttributes.BonusRange.Value + stolenRange));
		return stolenRange;
	}

	public static int StealTurnBarTime(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		float stolenTime = Math.Min(value, targetUnit.BattleAttributes.TurnBar.Current);
		if (stolenTime <= 0f)
		{
			return 0;
		}

		targetUnit.BattleAttributes.TurnBar.Decrease(stolenTime);
		casterUnit.BattleAttributes.TurnBar.Increase(stolenTime);
		return (int)stolenTime;
	}
}
