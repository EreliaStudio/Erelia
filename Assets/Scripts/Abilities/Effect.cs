using System;
using System.Collections.Generic;
using UnityEngine;
using static EffectUtility;

[Serializable]
public abstract class Effect
{
	public abstract void Apply(BattleObject caster, BattleObject target, BattleContext battleContext);
}

[Serializable]
public class ApplyStatusEffect : Effect
{
	public Status Status;
	public Duration Duration = new Duration();
	public int StackCount = 1;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Status == null || StackCount <= 0)
		{
			return;
		}

		targetUnit.Statuses.Add(Status, StackCount, Duration.Clone(Duration));
	}
}

[Serializable]
public class RemoveStatusEffect : Effect
{
	public Status Status;
	public int StackCount = 1;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Status == null)
		{
			return;
		}

		targetUnit.Statuses.Remove(Status, Math.Max(1, StackCount));
	}
}

[Serializable]
public class ReviveEffect : Effect
{
	public int HealthRestored = 1;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || !targetUnit.IsDefeated)
		{
			return;
		}

		int restoredHealth = MathFormula.ComputeHealing(
			(caster as BattleUnit)?.SourceUnit?.Attributes,
			new MathFormula.HealingInput
			{
				BaseHealing = Math.Max(1, HealthRestored)
			});

		targetUnit.BattleAttributes.Health.SetCurrent(Math.Max(1, restoredHealth));
	}
}

[Serializable]
public class CleanseEffect : Effect
{
	public List<string> TagsToCleanse = new List<string>();

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || TagsToCleanse == null || TagsToCleanse.Count == 0)
		{
			return;
		}

		targetUnit.Statuses.Remove(TagsToCleanse);
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

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Value == 0)
		{
			return;
		}

		switch (ResourceTargeted)
		{
			case Target.ActionPoint:
				if (Value > 0)
				{
					targetUnit.BattleAttributes.ActionPoints.Increase(Value);
				}
				else
				{
					targetUnit.BattleAttributes.ActionPoints.Decrease(-Value);
				}
				break;

			case Target.MovementPoint:
				if (Value > 0)
				{
					targetUnit.BattleAttributes.MovementPoints.Increase(Value);
				}
				else
				{
					targetUnit.BattleAttributes.MovementPoints.Decrease(-Value);
				}
				break;

			case Target.Range:
				targetUnit.BattleAttributes.BonusRange.Set(Math.Max(0, targetUnit.BattleAttributes.BonusRange.Value + Value));
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

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (battleContext == null ||
			caster == null ||
			target is not BattleUnit targetUnit ||
			Force <= 0)
		{
			return;
		}

		if (!battleContext.Board.Runtime.TryGetPosition(caster, out Vector3Int casterPosition))
		{
			return;
		}

		if (!battleContext.Board.Runtime.TryGetPosition(targetUnit, out Vector3Int targetPosition))
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
			if (!battleContext.TryMoveUnit(targetUnit, nextPosition))
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
	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (battleContext == null ||
			caster is not BattleUnit casterUnit ||
			target is not BattleUnit targetUnit)
		{
			return;
		}

		battleContext.TrySwapUnits(casterUnit, targetUnit);
	}
}

[Serializable]
public class TeleportEffect : Effect
{
	public Vector3Int Destination = Vector3Int.zero;
	public bool RelativeToCaster = true;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (battleContext == null || target is not BattleUnit targetUnit)
		{
			return;
		}

		Vector3Int destination = Destination;

		if (RelativeToCaster && caster != null)
		{
			if (battleContext.Board.Runtime.TryGetPosition(caster, out Vector3Int casterPosition))
			{
				destination += casterPosition;
			}
		}

		battleContext.TryPlaceUnit(targetUnit, destination);
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

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (caster is not BattleUnit casterUnit || target is not BattleUnit targetUnit || Value <= 0)
		{
			return;
		}

		switch (ResourceTargeted)
		{
			case Target.Health:
				StealHealth(casterUnit, targetUnit, Value);
				break;

			case Target.ActionPoint:
				StealActionPoints(casterUnit, targetUnit, Value);
				break;

			case Target.MovementPoint:
				StealMovementPoints(casterUnit, targetUnit, Value);
				break;

			case Target.Range:
				StealRange(casterUnit, targetUnit, Value);
				break;

			case Target.Stamina:
				StealTurnBarTime(casterUnit, targetUnit, Value);
				break;
		}
	}
}

[Serializable]
public class ConsumeStatus : Effect
{
	public Status Status;
	public int NbOfStackConsumed = 1;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Status == null)
		{
			return;
		}

		targetUnit.Statuses.Remove(Status, Math.Max(1, NbOfStackConsumed));
	}
}

[Serializable]
public class AdjustTurnBarTimeEffect : Effect
{
	public float Delta = 1f;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Mathf.Approximately(Delta, 0f))
		{
			return;
		}

		float adjustedDelta = MathFormula.ComputeTurnBarTimeDelta(
			Delta,
			targetUnit.SourceUnit?.Attributes);

		if (adjustedDelta > 0f)
		{
			targetUnit.BattleAttributes.TurnBar.Increase(adjustedDelta);
		}
		else
		{
			targetUnit.BattleAttributes.TurnBar.Decrease(-adjustedDelta);
		}
	}
}

[Serializable]
public class AdjustTurnBarDurationEffect : Effect
{
	public float Delta = 1f;

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Mathf.Approximately(Delta, 0f))
		{
			return;
		}

		float adjustedDelta = MathFormula.ComputeTurnBarDurationDelta(
			Delta,
			targetUnit.SourceUnit?.Attributes);

		targetUnit.BattleAttributes.TurnBar.SetMax(Math.Max(MathFormula.MinimumTurnBarDuration, targetUnit.BattleAttributes.TurnBar.Max + adjustedDelta));
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

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Input.BaseDamage <= 0)
		{
			return;
		}

		Attributes casterAttributes = (caster as BattleUnit)?.SourceUnit?.Attributes;
		Attributes targetAttributes = targetUnit.SourceUnit?.Attributes;
		int computedDamage = MathFormula.ComputeDamage(casterAttributes, targetAttributes, Input);

		if (computedDamage <= 0)
		{
			return;
		}

		int previousHealth = targetUnit.BattleAttributes.Health.Current;
		targetUnit.BattleAttributes.Health.Decrease(computedDamage);
		int appliedDamage = previousHealth - targetUnit.BattleAttributes.Health.Current;
		if (appliedDamage <= 0 || caster is not BattleUnit casterUnit)
		{
			return;
		}

		int selfHealing = MathFormula.ComputeVampirismHealing(casterAttributes, Input.DamageKind, appliedDamage);
		if (selfHealing > 0)
		{
			casterUnit.BattleAttributes.Health.Increase(selfHealing);
		}
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

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (target is not BattleUnit targetUnit || Input.BaseHealing <= 0)
		{
			return;
		}

		int computedHealing = MathFormula.ComputeHealing(
			(caster as BattleUnit)?.SourceUnit?.Attributes,
			Input);

		if (computedHealing <= 0)
		{
			return;
		}

		targetUnit.BattleAttributes.Health.Increase(computedHealing);
	}
}

[Serializable]
public class PlaceInteractiveObjectEffect : Effect
{
	public InteractionObject InteractionObject;
	public Duration Duration = new Duration();

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (battleContext == null || InteractionObject == null)
		{
			return;
		}

		if (!TryResolveAnchorPosition(battleContext, caster, target, out Vector3Int anchorPosition))
		{
			return;
		}

		BattleInteractiveObject interactiveObject = new BattleInteractiveObject
		{
			Side = caster != null ? caster.Side : BattleSide.Neutral,
			InteractionObject = InteractionObject,
			Tags = InteractionObject.Tags != null ? new List<string>(InteractionObject.Tags) : new List<string>(),
			RemainingDuration = Duration.Clone(Duration)
		};

		battleContext.Board.Runtime.TryAddInteractiveObject(interactiveObject, anchorPosition);
	}
}

[Serializable]
public class RemoveInteractiveObjectEffect : Effect
{
	public List<string> Tags = new List<string>();

	public override void Apply(BattleObject caster, BattleObject target, BattleContext battleContext)
	{
		if (battleContext == null || Tags == null || Tags.Count == 0)
		{
			return;
		}

		if (TryResolveAnchorPosition(battleContext, caster, target, out Vector3Int anchorPosition))
		{
			battleContext.Board.Runtime.RemoveInteractiveObjectsByTags(anchorPosition, Tags);
		}
		else
		{
			battleContext.Board.Runtime.RemoveInteractiveObjectsByTags(Tags);
		}
	}
}

internal static class EffectUtility
{
	public static bool TryResolveAnchorPosition(BattleContext battleContext, BattleObject caster, BattleObject target, out Vector3Int position)
	{
		position = default;

		if (battleContext == null)
		{
			return false;
		}

		if (target != null && battleContext.Board.Runtime.TryGetPosition(target, out position))
		{
			return true;
		}

		if (caster != null && battleContext.Board.Runtime.TryGetPosition(caster, out position))
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
