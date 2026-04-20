using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleContext
{
	private readonly List<BattleUnit> playerUnits = new();
	private readonly List<BattleUnit> enemyUnits = new();
	private readonly List<BattleUnit> allUnits = new();
	private readonly List<Vector3Int> playerPlacementCells = new();
	private readonly List<Vector3Int> enemyPlacementCells = new();

	public BattleSetup Setup { get; }
	public BoardData Board { get; }
	public IReadOnlyList<BattleUnit> PlayerUnits => playerUnits;
	public IReadOnlyList<BattleUnit> EnemyUnits => enemyUnits;
	public IReadOnlyList<BattleUnit> AllUnits => allUnits;
	public IReadOnlyList<Vector3Int> PlayerPlacementCells => playerPlacementCells;
	public IReadOnlyList<Vector3Int> EnemyPlacementCells => enemyPlacementCells;
	public BattleUnit ActiveUnit { get; set; }
	public BattleAction PendingAction { get; set; }
	public BattleSide Winner { get; private set; } = BattleSide.Neutral;
	public BattleResult Result { get; private set; } = BattleResult.None;

	public BattleContext(BattleSetup p_setup)
	{
		Setup = p_setup ?? throw new ArgumentNullException(nameof(p_setup));
		Board = p_setup.Board ?? throw new ArgumentNullException(nameof(p_setup.Board));

		InitializeUnits(p_setup.PlayerTeam, BattleSide.Player, playerUnits);
		InitializeUnits(p_setup.EnemyTeam, BattleSide.Enemy, enemyUnits);
	}

	public void ClearRuntime()
	{
		Board.Runtime.Clear();
		ActiveUnit = null;
		PendingAction = null;
		Winner = BattleSide.Neutral;
		Result = BattleResult.None;

		for (int index = 0; index < allUnits.Count; index++)
		{
			allUnits[index].BattleAttributes.Setup(allUnits[index].SourceUnit.Attributes);
		}

		playerPlacementCells.Clear();
		enemyPlacementCells.Clear();
	}

	public IEnumerable<BattleUnit> GetUnits(BattleSide p_side)
	{
		return p_side switch
		{
			BattleSide.Player => playerUnits,
			BattleSide.Enemy => enemyUnits,
			_ => Array.Empty<BattleUnit>()
		};
	}

	public IEnumerable<BattleUnit> GetOpponents(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return Array.Empty<BattleUnit>();
		}

		return p_unit.Side == BattleSide.Player ? enemyUnits : playerUnits;
	}

	public bool HasLivingUnits(BattleSide p_side)
	{
		foreach (BattleUnit unit in GetUnits(p_side))
		{
			if (unit != null && !unit.IsDefeated)
			{
				return true;
			}
		}

		return false;
	}

	public bool TryGetFirstLivingOpponent(BattleUnit p_sourceUnit, out BattleUnit p_targetUnit)
	{
		foreach (BattleUnit unit in GetOpponents(p_sourceUnit))
		{
			if (unit != null && !unit.IsDefeated)
			{
				p_targetUnit = unit;
				return true;
			}
		}

		p_targetUnit = null;
		return false;
	}

	public void RefillTurnResources(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		p_unit.BattleAttributes.ActionPoints.Reset();
		p_unit.BattleAttributes.MovementPoints.Reset();
	}

	public bool TryResolveBattleResult()
	{
		bool hasPlayerUnits = HasLivingUnits(BattleSide.Player);
		bool hasEnemyUnits = HasLivingUnits(BattleSide.Enemy);

		if (hasPlayerUnits && hasEnemyUnits)
		{
			Winner = BattleSide.Neutral;
			Result = BattleResult.None;
			return false;
		}

		if (hasPlayerUnits == hasEnemyUnits)
		{
			Winner = BattleSide.Neutral;
			Result = BattleResult.Draw;
			return true;
		}

		Winner = hasPlayerUnits ? BattleSide.Player : BattleSide.Enemy;
		Result = hasPlayerUnits ? BattleResult.PlayerVictory : BattleResult.EnemyVictory;
		return true;
	}

	public bool TryAutoPlaceTeams()
	{
		ClearRuntime();

		bool playerPlaced = TryPlaceLine(playerUnits, true);
		bool enemyPlaced = TryPlaceLine(enemyUnits, false);
		return playerPlaced || enemyPlaced;
	}

	public void AssignPlacementAreas(IReadOnlyList<Vector3Int> p_playerCells, IReadOnlyList<Vector3Int> p_enemyCells)
	{
		playerPlacementCells.Clear();
		enemyPlacementCells.Clear();

		if (p_playerCells != null)
		{
			playerPlacementCells.AddRange(p_playerCells);
		}

		if (p_enemyCells != null)
		{
			enemyPlacementCells.AddRange(p_enemyCells);
		}
	}

	public bool TryPlaceUnitsInCells(IReadOnlyList<BattleUnit> p_units, IReadOnlyList<Vector3Int> p_cells)
	{
		if (p_units == null || p_cells == null || p_units.Count == 0 || p_cells.Count == 0)
		{
			return false;
		}

		int placedCount = 0;
		for (int unitIndex = 0; unitIndex < p_units.Count; unitIndex++)
		{
			BattleUnit unit = p_units[unitIndex];
			if (unit == null)
			{
				continue;
			}

			for (int cellIndex = 0; cellIndex < p_cells.Count; cellIndex++)
			{
				Vector3Int cell = p_cells[cellIndex];
				if (!Board.TryPlace(unit, cell))
				{
					continue;
				}

				placedCount++;
				break;
			}
		}

		return placedCount > 0;
	}

	private void InitializeUnits(IReadOnlyList<CreatureUnit> p_sourceUnits, BattleSide p_side, List<BattleUnit> p_targetList)
	{
		if (p_sourceUnits == null)
		{
			return;
		}

		for (int index = 0; index < p_sourceUnits.Count; index++)
		{
			CreatureUnit sourceUnit = p_sourceUnits[index];
			if (sourceUnit == null)
			{
				continue;
			}

			var battleUnit = new BattleUnit(sourceUnit, p_side);
			p_targetList.Add(battleUnit);
			allUnits.Add(battleUnit);
		}
	}

	private bool TryPlaceLine(IReadOnlyList<BattleUnit> p_units, bool p_fromLeftToRight)
	{
		if (p_units == null || p_units.Count == 0)
		{
			return false;
		}

		int sizeX = Board.Terrain.SizeX;
		int sizeY = Board.Terrain.SizeY;
		int sizeZ = Board.Terrain.SizeZ;
		int startX = p_fromLeftToRight ? 0 : sizeX - 1;
		int endX = p_fromLeftToRight ? sizeX : -1;
		int stepX = p_fromLeftToRight ? 1 : -1;
		int placedCount = 0;

		for (int unitIndex = 0; unitIndex < p_units.Count; unitIndex++)
		{
			BattleUnit unit = p_units[unitIndex];
			if (unit == null)
			{
				continue;
			}

			bool placed = false;
			for (int x = startX; x != endX && !placed; x += stepX)
			{
				for (int z = 0; z < sizeZ && !placed; z++)
				{
					for (int y = 0; y < sizeY && !placed; y++)
					{
						Vector3Int position = new Vector3Int(x, y, z);
						if (!Board.TryPlace(unit, position))
						{
							continue;
						}

						placed = true;
						placedCount++;
					}
				}
			}
		}

		return placedCount > 0;
	}
}
