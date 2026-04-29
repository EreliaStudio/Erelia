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

		context.TargetUnit.BattleAttributes.Health.SetCurrent(Math.Max(1, restoredHealth));
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
				if (Value > 0)
				{
					context.TargetUnit.BattleAttributes.ActionPoints.Increase(Value);
				}
				else
				{
					context.TargetUnit.BattleAttributes.ActionPoints.Decrease(-Value);
				}
				break;

			case Target.MovementPoint:
				if (Value > 0)
				{
					context.TargetUnit.BattleAttributes.MovementPoints.Increase(Value);
				}
				else
				{
					context.TargetUnit.BattleAttributes.MovementPoints.Decrease(-Value);
				}
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
		for (int index = 0; index < Force; index++)
		{
			Vector3Int nextPosition = currentPosition + step;
			if (!context.BattleContext.TryMoveUnit(context.TargetUnit, nextPosition))
			{
				break;
			}

			currentPosition = nextPosition;
		}
	}
}

[Serializable]
public class SwapPositionEffect : Effect
{
	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null ||
			context.SourceUnit == null ||
			context.TargetUnit == null)
		{
			return;
		}

		context.BattleContext.TrySwapUnits(context.SourceUnit, context.TargetUnit);
	}
}

[Serializable]
public class TeleportEffect : Effect
{
	public Vector3Int Destination = Vector3Int.zero;
	public bool RelativeToCaster = true;

	public override void Apply(BattleAbilityExecutionContext context)
	{
		if (context?.BattleContext == null || context.TargetUnit == null)
		{
			return;
		}

		Vector3Int destination = Destination;

		if (RelativeToCaster && context.SourceObject != null)
		{
			if (context.BattleContext.Board.Runtime.TryGetPosition(context.SourceObject, out Vector3Int casterPosition))
			{
				destination += casterPosition;
			}
		}
		else if (!RelativeToCaster)
		{
			destination += context.AnchorCell;
		}

		context.BattleContext.TryPlaceUnit(context.TargetUnit, destination);
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

		switch (ResourceTargeted)
		{
			case Target.Health:
				StealHealth(context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.ActionPoint:
				StealActionPoints(context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.MovementPoint:
				StealMovementPoints(context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.Range:
				StealRange(context.SourceUnit, context.TargetUnit, Value);
				break;

			case Target.Stamina:
				StealTurnBarTime(context.SourceUnit, context.TargetUnit, Value);
				break;
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

		int previousHealth = context.TargetUnit.BattleAttributes.Health.Current;
		context.TargetUnit.BattleAttributes.Health.Decrease(computedDamage);
		int appliedDamage = previousHealth - context.TargetUnit.BattleAttributes.Health.Current;

		if (appliedDamage <= 0)
		{
			return;
		}

		int selfHealing = MathFormula.ComputeVampirismHealing(casterAttributes, Input.DamageKind, appliedDamage);
		if (selfHealing > 0 && context.SourceUnit != null)
		{
			context.SourceUnit.BattleAttributes.Health.Increase(selfHealing);
		}

		context.SourceUnit?.RecordFeatEvent(new DealDamageRequirement.Event { Amount = appliedDamage });
		context.SourceUnit?.RecordFeatEvent(new MaxSingleHitDamageRequirement.Event { Amount = appliedDamage });
		context.TargetUnit.RecordFeatEvent(new TakeDamageRequirement.Event { Amount = appliedDamage });
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

		context.TargetUnit.BattleAttributes.Health.Increase(computedHealing);
		context.SourceUnit?.RecordFeatEvent(new HealHealthRequirement.Event { Amount = computedHealing });
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

	public static void StealHealth(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		int previousTargetHealth = targetUnit.BattleAttributes.Health.Current;
		targetUnit.BattleAttributes.Health.Decrease(value);

		int stolenHealth = previousTargetHealth - targetUnit.BattleAttributes.Health.Current;
		if (stolenHealth <= 0)
		{
			return;
		}

		casterUnit.BattleAttributes.Health.Increase(stolenHealth);
	}

	public static void StealActionPoints(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		int stolenPoints = Math.Min(value, targetUnit.BattleAttributes.ActionPoints.Current);
		if (stolenPoints <= 0)
		{
			return;
		}

		targetUnit.BattleAttributes.ActionPoints.Decrease(stolenPoints);
		casterUnit.BattleAttributes.ActionPoints.Increase(stolenPoints);
	}

	public static void StealMovementPoints(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		int stolenPoints = Math.Min(value, targetUnit.BattleAttributes.MovementPoints.Current);
		if (stolenPoints <= 0)
		{
			return;
		}

		targetUnit.BattleAttributes.MovementPoints.Decrease(stolenPoints);
		casterUnit.BattleAttributes.MovementPoints.Increase(stolenPoints);
	}

	public static void StealRange(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		int stolenRange = Math.Min(value, targetUnit.BattleAttributes.BonusRange.Value);
		if (stolenRange <= 0)
		{
			return;
		}

		targetUnit.BattleAttributes.BonusRange.Set(Math.Max(0, targetUnit.BattleAttributes.BonusRange.Value - stolenRange));
		casterUnit.BattleAttributes.BonusRange.Set(Math.Max(0, casterUnit.BattleAttributes.BonusRange.Value + stolenRange));
	}

	public static void StealTurnBarTime(BattleUnit casterUnit, BattleUnit targetUnit, int value)
	{
		float stolenTime = Math.Min(value, targetUnit.BattleAttributes.TurnBar.Current);
		if (stolenTime <= 0f)
		{
			return;
		}

		targetUnit.BattleAttributes.TurnBar.Decrease(stolenTime);
		casterUnit.BattleAttributes.TurnBar.Increase(stolenTime);
	}
}
