using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	public static class TargetingUtility
	{
		public static int ApplyAttack(
			Erelia.Battle.BattleState battle,
			Erelia.Battle.Unit.Presenter caster,
			Erelia.Battle.Attack attack,
			Vector3Int castCell)
		{
			if (battle == null ||
				caster == null ||
				attack == null ||
				!caster.IsAlive ||
				!caster.IsPlaced)
			{
				return 0;
			}

			List<Vector3Int> affectedCells = BuildAreaOfEffectCoordinates(
				battle,
				castCell,
				attack.AreaOfEffectRange);
			IReadOnlyList<Erelia.Battle.Effects.AttackEffect> effects = attack.Effects;
			if (affectedCells.Count == 0 || effects == null || effects.Count == 0)
			{
				return 0;
			}

			var processedUnits = new HashSet<Erelia.Battle.Unit.Presenter>();
			int affectedUnitCount = 0;

			for (int i = 0; i < affectedCells.Count; i++)
			{
				Vector3Int affectedCell = affectedCells[i];
				if (!battle.TryGetPlacedUnitAtCell(affectedCell, out Erelia.Battle.Unit.Presenter targetUnit) ||
					targetUnit == null ||
					!processedUnits.Add(targetUnit))
				{
					continue;
				}

				affectedUnitCount++;

				for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++)
				{
					Erelia.Battle.Effects.AttackEffect effect = effects[effectIndex];
					int previousHealth = targetUnit.CurrentHealth;
					effect?.ApplyTo(caster, targetUnit, castCell);
					battle.FeatProgressTracker.RegisterHealthChange(
						caster,
						targetUnit,
						previousHealth,
						targetUnit.CurrentHealth);
				}
			}

			return affectedUnitCount;
		}

		public static List<Vector3Int> BuildRangeCoordinates(
			Erelia.Battle.BattleState battle,
			Vector3Int originCell,
			int range,
			Erelia.Battle.RangePattern rangePattern)
		{
			var coordinates = new List<Vector3Int>();
			IReadOnlyList<Vector3Int> acceptableCoordinates = battle?.AcceptableCoordinates;
			if (acceptableCoordinates == null)
			{
				return coordinates;
			}

			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (!IsWithinRange(coordinate - originCell, range, rangePattern))
				{
					continue;
				}

				coordinates.Add(coordinate);
			}

			return coordinates;
		}

		public static List<Vector3Int> BuildAreaOfEffectCoordinates(
			Erelia.Battle.BattleState battle,
			Vector3Int originCell,
			int areaOfEffectRange)
		{
			return BuildRangeCoordinates(
				battle,
				originCell,
				areaOfEffectRange,
				Erelia.Battle.RangePattern.Circle);
		}

		public static Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter> BuildTargetableUnits(
			Erelia.Battle.BattleState battle,
			Erelia.Battle.Unit.Presenter actingUnit,
			Erelia.Battle.Attack attack)
		{
			var targets = new Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter>();
			if (battle == null ||
				actingUnit == null ||
				attack == null ||
				!actingUnit.IsAlive ||
				!actingUnit.IsPlaced)
			{
				return targets;
			}

			IReadOnlyList<Erelia.Battle.Unit.Presenter> units = battle.Units;
			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter candidate = units[i];
				if (!IsValidTarget(actingUnit, candidate, attack))
				{
					continue;
				}

				Vector3Int delta = candidate.Cell - actingUnit.Cell;
				if (!IsWithinRange(delta, attack.Range, attack.RangePattern))
				{
					continue;
				}

				targets[candidate.Cell] = candidate;
			}

			return targets;
		}

		private static bool IsValidTarget(
			Erelia.Battle.Unit.Presenter actingUnit,
			Erelia.Battle.Unit.Presenter candidate,
			Erelia.Battle.Attack attack)
		{
			if (actingUnit == null ||
				candidate == null ||
				attack == null ||
				!candidate.IsAlive ||
				!candidate.IsPlaced)
			{
				return false;
			}

			switch (attack.TargetType)
			{
				case Erelia.Battle.TargetType.Enemy:
					return candidate.Side != actingUnit.Side;

				case Erelia.Battle.TargetType.Ally:
					return candidate.Side == actingUnit.Side;

				case Erelia.Battle.TargetType.Both:
					return true;

				default:
					return false;
			}
		}

		public static bool IsWithinRange(
			Vector3Int delta,
			int range,
			Erelia.Battle.RangePattern rangePattern)
		{
			int horizontalX = Mathf.Abs(delta.x);
			int horizontalZ = Mathf.Abs(delta.z);
			int clampedRange = Mathf.Max(0, range);

			switch (rangePattern)
			{
				case Erelia.Battle.RangePattern.StraightLine:
					if (horizontalX != 0 && horizontalZ != 0)
					{
						return false;
					}

					return Mathf.Max(horizontalX, horizontalZ) <= clampedRange;

				case Erelia.Battle.RangePattern.Diagonal:
					return horizontalX == horizontalZ && horizontalX <= clampedRange;

				case Erelia.Battle.RangePattern.Circle:
					return horizontalX * horizontalX + horizontalZ * horizontalZ <= clampedRange * clampedRange;

				default:
					return false;
			}
		}
	}
}



