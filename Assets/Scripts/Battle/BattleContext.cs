using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleContext
{
	private readonly List<BattleUnit> playerUnits = new();
	private readonly List<BattleUnit> enemyUnits = new();
	private readonly List<BattleUnit> allUnits = new();

	public event Action<BattleUnit> UnitRegistered;
	public event Action<BattleUnit> UnitRemoved;

	public BoardData Board { get; }
	public Vector3 PlayerWorldPosition { get; }
	public IReadOnlyList<BattleUnit> PlayerUnits => playerUnits;
	public IReadOnlyList<BattleUnit> EnemyUnits => enemyUnits;
	public IReadOnlyList<BattleUnit> AllUnits => allUnits;

	public BattleContext(
		IReadOnlyList<CreatureUnit> p_playerTeam,
		IReadOnlyList<EncounterUnit> p_enemyTeam,
		BoardData p_board,
		Vector3 p_playerWorldPosition)
	{
		if (p_playerTeam == null)
		{
			throw new ArgumentNullException(nameof(p_playerTeam));
		}

		if (p_enemyTeam == null)
		{
			throw new ArgumentNullException(nameof(p_enemyTeam));
		}

		Board = p_board ?? throw new ArgumentNullException(nameof(p_board));
		PlayerWorldPosition = p_playerWorldPosition;

		InitializeUnits(p_playerTeam, BattleSide.Player, playerUnits);
		InitializeUnits(p_enemyTeam, BattleSide.Enemy, enemyUnits);
	}

	public void ClearRuntime()
	{
		Board.Runtime.Clear();

		for (int index = 0; index < allUnits.Count; index++)
		{
			allUnits[index].BattleAttributes.Setup(allUnits[index].SourceUnit.Attributes);
			allUnits[index].ClearBoardPosition();
		}
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

	public bool TryPlaceUnit(BattleUnit p_unit, Vector3Int p_cell)
	{
		bool wasAlreadyPlaced = p_unit != null && p_unit.HasBoardPosition;
		if (p_unit == null || !Board.TryPlace(p_unit, p_cell))
		{
			return false;
		}

		p_unit.SetBoardPosition(p_cell);
		if (!wasAlreadyPlaced)
		{
			UnitRegistered?.Invoke(p_unit);
		}

		return true;
	}

	public bool TryMoveUnit(BattleUnit p_unit, Vector3Int p_cell)
	{
		if (p_unit == null || !Board.TryMove(p_unit, p_cell))
		{
			return false;
		}

		p_unit.SetBoardPosition(p_cell);
		return true;
	}

	public bool TrySwapUnits(BattleUnit p_firstUnit, BattleUnit p_secondUnit)
	{
		if (p_firstUnit == null || p_secondUnit == null || !Board.Runtime.SwapUnits(p_firstUnit, p_secondUnit))
		{
			return false;
		}

		if (Board.TryGetPosition(p_firstUnit, out Vector3Int firstPosition))
		{
			p_firstUnit.SetBoardPosition(firstPosition);
		}

		if (Board.TryGetPosition(p_secondUnit, out Vector3Int secondPosition))
		{
			p_secondUnit.SetBoardPosition(secondPosition);
		}

		return true;
	}

	public void RemoveUnit(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		Board.Remove(p_unit);
		p_unit.ClearBoardPosition();
		UnitRemoved?.Invoke(p_unit);
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
			if (sourceUnit == null || sourceUnit.Species == null)
			{
				continue;
			}

			var battleUnit = new BattleUnit(sourceUnit, p_side);
			p_targetList.Add(battleUnit);
			allUnits.Add(battleUnit);
		}
	}
}
