using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Attack
{
	public static class TargetingUtility
	{
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

		private static bool IsWithinRange(
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
