using System;
using System.Collections.Generic;
using UnityEngine;

public static class BattleTargetingRules
{
	public static IReadOnlyList<Vector3Int> GetAffectedCells(BattleContext battleContext, Ability ability, AbilityCastLegality legality, Vector3Int? casterCell = null)
	{
		return GetAffectedCells(battleContext, ability, legality.TargetCell, casterCell);
	}

	public static IReadOnlyList<Vector3Int> GetAffectedCells(BattleContext battleContext, Ability ability, Vector3Int anchorCell, Vector3Int? casterCell = null)
	{
		if (battleContext?.Board == null || ability == null)
		{
			return Array.Empty<Vector3Int>();
		}

		if (ability.AreaOfEffect?.Type == Ability.AreaOfEffectDefinition.Shape.Line)
		{
			return GetLineCells(battleContext, ability, anchorCell, casterCell);
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

	private static IReadOnlyList<Vector3Int> GetLineCells(BattleContext battleContext, Ability ability, Vector3Int anchorCell, Vector3Int? casterCell)
	{
		int length = Math.Max(0, ability.AreaOfEffect?.Value ?? 0);
		List<Vector3Int> cells = new List<Vector3Int>();

		Vector3Int direction = Vector3Int.zero;
		if (casterCell.HasValue && casterCell.Value != anchorCell)
		{
			Vector3Int delta = anchorCell - casterCell.Value;
			if (Math.Abs(delta.x) >= Math.Abs(delta.z))
			{
				direction = new Vector3Int(Math.Sign(delta.x), 0, 0);
			}
			else
			{
				direction = new Vector3Int(0, 0, Math.Sign(delta.z));
			}
		}

		for (int i = 0; i <= length; i++)
		{
			Vector3Int cell = anchorCell + direction * i;
			if (!battleContext.Board.IsInside(cell))
			{
				break;
			}

			cells.Add(cell);
		}

		return cells;
	}

	public static IReadOnlyList<BattleObject> GetObjectsAtCell(BattleContext battleContext, Vector3Int cell)
	{
		if (battleContext == null)
		{
			return Array.Empty<BattleObject>();
		}

		return battleContext.GetObjectsAt(cell);
	}

	public static IReadOnlyList<BattleObject> GetAffectedObjects(BattleContext battleContext, Ability ability, Vector3Int anchorCell, Vector3Int? casterCell = null)
	{
		if (battleContext == null || ability == null)
		{
			return Array.Empty<BattleObject>();
		}

		HashSet<BattleObject> uniqueObjects = new HashSet<BattleObject>();
		IReadOnlyList<Vector3Int> affectedCells = GetAffectedCells(battleContext, ability, anchorCell, casterCell);
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
