using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleTargetingRules
{
	public static IReadOnlyList<Vector3Int> GetAffectedCells(BattleContext battleContext, Ability ability, AbilityCastLegality legality)
	{
		return GetAffectedCells(battleContext, ability, legality.TargetCell);
	}

	public static IReadOnlyList<Vector3Int> GetAffectedCells(BattleContext battleContext, Ability ability, Vector3Int anchorCell)
	{
		if (battleContext?.Board == null || ability == null)
		{
			return Array.Empty<Vector3Int>();
		}

		HashSet<Vector3Int> uniqueCells = new HashSet<Vector3Int>();
		int areaValue = Math.Max(0, ability.AreaOfEffect?.Value ?? 0);

		for (int offsetX = -areaValue; offsetX <= areaValue; offsetX++)
		{
			for (int offsetZ = -areaValue; offsetZ <= areaValue; offsetZ++)
			{
				if (!IsIncludedInArea(ability, offsetX, offsetZ, areaValue))
				{
					continue;
				}

				Vector3Int cell = new Vector3Int(anchorCell.x + offsetX, anchorCell.y, anchorCell.z + offsetZ);
				if (!battleContext.Board.IsInside(cell))
				{
					continue;
				}

				uniqueCells.Add(cell);
			}
		}

		return new List<Vector3Int>(uniqueCells);
	}

	public static IReadOnlyList<BattleObject> GetObjectsAtCell(BattleContext battleContext, Vector3Int cell)
	{
		if (battleContext == null)
		{
			return Array.Empty<BattleObject>();
		}

		return battleContext.GetObjectsAt(cell);
	}

	public static IReadOnlyList<BattleObject> GetAffectedObjects(BattleContext battleContext, Ability ability, Vector3Int anchorCell)
	{
		if (battleContext == null || ability == null)
		{
			return Array.Empty<BattleObject>();
		}

		HashSet<BattleObject> uniqueObjects = new HashSet<BattleObject>();
		IReadOnlyList<Vector3Int> affectedCells = GetAffectedCells(battleContext, ability, anchorCell);
		for (int cellIndex = 0; cellIndex < affectedCells.Count; cellIndex++)
		{
			IReadOnlyList<BattleObject> objectsAtCell = GetObjectsAtCell(battleContext, affectedCells[cellIndex]);
			for (int objectIndex = 0; objectIndex < objectsAtCell.Count; objectIndex++)
			{
				BattleObject battleObject = objectsAtCell[objectIndex];
				if (battleObject != null)
				{
					uniqueObjects.Add(battleObject);
				}
			}
		}

		return new List<BattleObject>(uniqueObjects);
	}

	private static bool IsIncludedInArea(Ability ability, int offsetX, int offsetZ, int areaValue)
	{
		return (ability.AreaOfEffect?.Type ?? Ability.AreaOfEffectDefinition.Shape.Square) switch
		{
			Ability.AreaOfEffectDefinition.Shape.Square => Math.Abs(offsetX) <= areaValue && Math.Abs(offsetZ) <= areaValue,
			Ability.AreaOfEffectDefinition.Shape.Cross => (offsetX == 0 || offsetZ == 0) && Math.Abs(offsetX) + Math.Abs(offsetZ) <= areaValue,
			Ability.AreaOfEffectDefinition.Shape.Circle => Math.Abs(offsetX) + Math.Abs(offsetZ) <= areaValue,
			_ => false
		};
	}
}
