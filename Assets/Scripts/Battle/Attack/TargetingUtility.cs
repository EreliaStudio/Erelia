using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Attack
{
	public static class TargetingUtility
	{
		public static int ApplyAttack(
			Erelia.Battle.Data battleData,
			Erelia.Battle.Unit.Presenter caster,
			Erelia.Battle.Attack.Definition attack,
			Vector3Int castCell)
		{
			if (battleData == null ||
				caster == null ||
				attack == null ||
				!caster.IsAlive ||
				!caster.IsPlaced)
			{
				return 0;
			}

			List<Vector3Int> affectedCells = BuildAreaOfEffectCoordinates(
				battleData,
				castCell,
				attack.AreaOfEffectRange);
			IReadOnlyList<Erelia.Battle.Attack.Effect.Definition> effects = attack.Effects;
			if (affectedCells.Count == 0 || effects == null || effects.Count == 0)
			{
				return 0;
			}

			var processedUnits = new HashSet<Erelia.Battle.Unit.Presenter>();
			int affectedUnitCount = 0;

			for (int i = 0; i < affectedCells.Count; i++)
			{
				Vector3Int affectedCell = affectedCells[i];
				if (!battleData.TryGetPlacedUnitAtCell(affectedCell, out Erelia.Battle.Unit.Presenter targetUnit) ||
					targetUnit == null ||
					!processedUnits.Add(targetUnit))
				{
					continue;
				}

				affectedUnitCount++;

				for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++)
				{
					Erelia.Battle.Attack.Effect.Definition effect = effects[effectIndex];
					int previousHealth = targetUnit.CurrentHealth;
					effect?.ApplyTo(caster, targetUnit, castCell);
					battleData.FeatProgressTracker.RegisterHealthChange(
						caster,
						targetUnit,
						previousHealth,
						targetUnit.CurrentHealth);
				}
			}

			return affectedUnitCount;
		}

		public static List<Vector3Int> BuildRangeCoordinates(
			Erelia.Battle.Data battleData,
			Vector3Int originCell,
			int range,
			Erelia.Battle.Attack.RangePattern rangePattern)
		{
			var coordinates = new List<Vector3Int>();
			IReadOnlyList<Vector3Int> acceptableCoordinates = battleData?.AcceptableCoordinates;
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
			Erelia.Battle.Data battleData,
			Vector3Int originCell,
			int areaOfEffectRange)
		{
			return BuildRangeCoordinates(
				battleData,
				originCell,
				areaOfEffectRange,
				Erelia.Battle.Attack.RangePattern.Circle);
		}

		public static Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter> BuildTargetableUnits(
			Erelia.Battle.Data battleData,
			Erelia.Battle.Unit.Presenter actingUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			var targets = new Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter>();
			if (battleData == null ||
				actingUnit == null ||
				attack == null ||
				!actingUnit.IsAlive ||
				!actingUnit.IsPlaced)
			{
				return targets;
			}

			IReadOnlyList<Erelia.Battle.Unit.Presenter> units = battleData.Units;
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
			Erelia.Battle.Attack.Definition attack)
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
				case Erelia.Battle.Attack.TargetType.Enemy:
					return candidate.Side != actingUnit.Side;

				case Erelia.Battle.Attack.TargetType.Ally:
					return candidate.Side == actingUnit.Side;

				case Erelia.Battle.Attack.TargetType.Both:
					return true;

				default:
					return false;
			}
		}

		public static bool IsWithinRange(
			Vector3Int delta,
			int range,
			Erelia.Battle.Attack.RangePattern rangePattern)
		{
			int horizontalX = Mathf.Abs(delta.x);
			int horizontalZ = Mathf.Abs(delta.z);
			int clampedRange = Mathf.Max(0, range);

			switch (rangePattern)
			{
				case Erelia.Battle.Attack.RangePattern.StraightLine:
					if (horizontalX != 0 && horizontalZ != 0)
					{
						return false;
					}

					return Mathf.Max(horizontalX, horizontalZ) <= clampedRange;

				case Erelia.Battle.Attack.RangePattern.Diagonal:
					return horizontalX == horizontalZ && horizontalX <= clampedRange;

				case Erelia.Battle.Attack.RangePattern.Circle:
					return horizontalX * horizontalX + horizontalZ * horizontalZ <= clampedRange * clampedRange;

				default:
					return false;
			}
		}
	}
}
