using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleContext
{
	public readonly List<BattleUnit> PlayerUnits = new();
	public readonly List<BattleUnit> EnemyUnits = new();
	private readonly List<BattleResourceChangeResult> pendingResourceChanges = new();

	public event Action<BattleUnit> UnitRegistered;
	public event Action<BattleUnit> UnitRemoved;
	public event Action<BattleUnit> UnitDefeated;

	public BoardData Board { get; }
	public PlacementStyle PlacementStyle { get; }
	public Vector3Int? ReturnWorldCell { get; }
	public TurnContext CurrentTurn { get; } = new();

	public BattleContext(
		IReadOnlyList<CreatureUnit> p_playerTeam,
		IReadOnlyList<EncounterUnit> p_enemyTeam,
		BoardData p_board,
		PlacementStyle p_placementStyle,
		bool p_allowsTaming = true,
		Vector3Int? p_returnWorldCell = null)
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
		PlacementStyle = p_placementStyle;
		ReturnWorldCell = p_returnWorldCell;

		InitializeUnits(p_playerTeam, BattleSide.Player, PlayerUnits, false);
		InitializeUnits(p_enemyTeam, BattleSide.Enemy, EnemyUnits, p_allowsTaming);
	}

	public void ClearRuntime()
	{
		pendingResourceChanges.Clear();
		Board.Runtime.Clear();
		CurrentTurn.End();

		for (int index = 0; index < PlayerUnits.Count; index++)
		{
			PlayerUnits[index].ResetBattleRuntimeState();
		}

		for (int index = 0; index < EnemyUnits.Count; index++)
		{
			EnemyUnits[index].ResetBattleRuntimeState();
		}
	}

	public IEnumerable<BattleUnit> GetUnits(BattleSide p_side)
	{
		return p_side switch
		{
			BattleSide.Player => PlayerUnits,
			BattleSide.Enemy => EnemyUnits,
			_ => Array.Empty<BattleUnit>()
		};
	}

	public IEnumerable<BattleUnit> GetOpponents(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return Array.Empty<BattleUnit>();
		}

		return p_unit.Side == BattleSide.Player ? EnemyUnits : PlayerUnits;
	}

	public bool HasLivingUnits(BattleSide p_side)
	{
		foreach (BattleUnit unit in GetUnits(p_side))
		{
			if (unit != null && unit.IsActiveInBattle)
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
			if (unit != null && unit.IsActiveInBattle)
			{
				p_targetUnit = unit;
				return true;
			}
		}

		p_targetUnit = null;
		return false;
	}

	public IReadOnlyList<BattleObject> GetObjectsAt(Vector3Int p_cell)
	{
		List<BattleObject> objects = new List<BattleObject>();

		if (Board.TryGetUnitAt(p_cell, out BattleUnit unit) && unit != null)
		{
			objects.Add(unit);
		}

		IReadOnlyList<BattleInteractiveObject> interactiveObjects = Board.Runtime.GetInteractiveObjects(p_cell);
		for (int index = 0; index < interactiveObjects.Count; index++)
		{
			BattleInteractiveObject interactiveObject = interactiveObjects[index];
			if (interactiveObject != null)
			{
				objects.Add(interactiveObject);
			}
		}

		return objects;
	}

	public bool TryPlaceUnit(BattleUnit p_unit, Vector3Int p_cell)
	{
		bool wasAlreadyPlaced = p_unit != null && p_unit.HasBoardPosition;
		if (p_unit == null || p_unit.HasLeftBattle || !Board.TryPlace(p_unit, p_cell))
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
		if (p_unit == null || p_unit.HasLeftBattle || !Board.TryMove(p_unit, p_cell))
		{
			return false;
		}

		p_unit.SetBoardPosition(p_cell);
		return true;
	}

	public bool TrySwapUnits(BattleUnit p_firstUnit, BattleUnit p_secondUnit)
	{
		if (p_firstUnit == null ||
			p_secondUnit == null ||
			p_firstUnit.HasLeftBattle ||
			p_secondUnit.HasLeftBattle ||
			!Board.Runtime.SwapUnits(p_firstUnit, p_secondUnit))
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

	internal void RecordResourceChange(BattleResourceChangeResult p_result)
	{
		if (p_result.Changed)
		{
			pendingResourceChanges.Add(p_result);
		}
	}

	internal List<BattleResourceChangeResult> ConsumePendingResourceChanges()
	{
		List<BattleResourceChangeResult> results = new List<BattleResourceChangeResult>(pendingResourceChanges);
		pendingResourceChanges.Clear();
		return results;
	}

	public void RemoveUnit(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		Board.Remove(p_unit);
		p_unit.ClearBoardPosition();
		p_unit.MarkLeftBattle();
		UnitRemoved?.Invoke(p_unit);
	}

	public void DefeatUnit(BattleUnit p_unit)
	{
		if (p_unit == null || !p_unit.IsDefeated)
		{
			return;
		}

		(p_unit as WildBattleUnit)?.MarkUntamable();

		if (p_unit.HasBoardPosition)
		{
			Board.Remove(p_unit);
			p_unit.ClearBoardPosition();
			UnitRemoved?.Invoke(p_unit);
		}

		UnitDefeated?.Invoke(p_unit);
	}

	private void InitializeUnits(IReadOnlyList<CreatureUnit> p_sourceUnits, BattleSide p_side, List<BattleUnit> p_targetList, bool p_wild)
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

			BattleUnit battleUnit;
			if (p_wild)
			{
				TamingProfile profile = sourceUnit.Species.TamingProfile;
				battleUnit = profile != null && profile.HasConditions
					? new WildBattleUnit(sourceUnit, p_side, profile)
					: new BattleUnit(sourceUnit, p_side);
			}
			else
			{
				battleUnit = new BattleUnit(sourceUnit, p_side);
			}

			p_targetList.Add(battleUnit);
		}
	}
}
