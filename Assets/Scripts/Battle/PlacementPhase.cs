using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlacementPhase : BattlePhase
{
	private readonly BoardPresenter boardPresenter;
	private readonly BattlePlayerController playerController;
	private BattleUnit selectedUnit;
	private bool hasConfirmedPlacement;

	public PlacementPhase(BattleContext p_context, BoardPresenter p_boardPresenter, BattlePlayerController p_playerController) : base(p_context)
	{
		boardPresenter = p_boardPresenter ?? throw new ArgumentNullException(nameof(p_boardPresenter));
		playerController = p_playerController ?? throw new ArgumentNullException(nameof(p_playerController));
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.Placement;

	public event Action PlacementConfirmed;

	public override void Enter()
	{
		hasConfirmedPlacement = false;
		Context.ClearRuntime();
		ResolvePlacementAreas();
		Context.TryPlaceUnitsInCells(Context.EnemyUnits, Context.EnemyPlacementCells);
		Context.TryRegisterInitialUnits();
		playerController.BindPlacement(boardPresenter, Context);
		playerController.PlacementController.HoveredCellChanged += OnHoveredCellChanged;
		playerController.PlacementController.PlacementRequested += OnPlacementRequested;
		SelectNextCreature();
		RenderPlayerPlacementMask();

		if (selectedUnit == null)
		{
			ConfirmPlacement();
		}
	}

	public override void Tick(float p_deltaTime)
	{
	}

	public override void Exit()
	{
		playerController.PlacementController.HoveredCellChanged -= OnHoveredCellChanged;
		playerController.PlacementController.PlacementRequested -= OnPlacementRequested;
		playerController.UnbindPlacement();
		Context.Board.ClearMask();
		boardPresenter.Rebuild();
	}

	private void ResolvePlacementAreas()
	{
		switch (Context.Board.PlacementStyle)
		{
			case BoardConfiguration.PlacementStyle.HalfBoard:
			default:
				AssignHalfBoardAreas();
				break;
		}
	}

	private void AssignHalfBoardAreas()
	{
		var playerCells = new List<Vector3Int>();
		var enemyCells = new List<Vector3Int>();
		int splitX = Mathf.CeilToInt(Context.Board.Terrain.SizeX * 0.5f);

		for (int x = 0; x < Context.Board.Terrain.SizeX; x++)
		{
			for (int z = 0; z < Context.Board.Terrain.SizeZ; z++)
			{
				for (int y = 0; y < Context.Board.Terrain.SizeY; y++)
				{
					Vector3Int cell = new Vector3Int(x, y, z);
					if (!Context.Board.IsStandable(cell))
					{
						continue;
					}

					if (x < splitX)
					{
						playerCells.Add(cell);
					}
					else
					{
						enemyCells.Add(cell);
					}
				}
			}
		}

		Context.AssignPlacementAreas(playerCells, enemyCells);
	}

	private void RenderPlayerPlacementMask()
	{
		Context.Board.ClearMask();

		for (int index = 0; index < Context.PlayerPlacementCells.Count; index++)
		{
			Context.Board.Terrain.MaskLayer.TryAddMask(Context.PlayerPlacementCells[index], VoxelMask.Placement);
		}

		if (playerController.PlacementController.HoveredCell.HasValue)
		{
			Context.Board.Terrain.MaskLayer.TryAddMask(playerController.PlacementController.HoveredCell.Value, VoxelMask.Selected);
		}

		boardPresenter.Rebuild();
	}

	private void OnHoveredCellChanged(Vector3Int? p_hoveredCell)
	{
		RenderPlayerPlacementMask();
	}

	private void OnPlacementRequested(BattleUnit p_unit, Vector3Int p_cell)
	{
		if (hasConfirmedPlacement || p_unit == null || p_unit != selectedUnit || !CanPlaceSelectedUnitAt(p_cell))
		{
			return;
		}

		if (!Context.TryPlaceUnit(p_unit, p_cell))
		{
			return;
		}

		SelectNextCreature();
		RenderPlayerPlacementMask();

		if (selectedUnit == null)
		{
			ConfirmPlacement();
		}
	}

	private bool CanPlaceSelectedUnitAt(Vector3Int p_cell)
	{
		if (selectedUnit == null)
		{
			return false;
		}

		for (int index = 0; index < Context.PlayerPlacementCells.Count; index++)
		{
			if (Context.PlayerPlacementCells[index] != p_cell)
			{
				continue;
			}

			return Context.Board.CanPlace(selectedUnit, p_cell);
		}

		return false;
	}

	private void SelectNextCreature()
	{
		selectedUnit = null;

		for (int index = 0; index < Context.PlayerUnits.Count; index++)
		{
			BattleUnit candidate = Context.PlayerUnits[index];
			if (candidate == null || Context.Board.TryGetPosition(candidate, out _))
			{
				continue;
			}

			selectedUnit = candidate;
			break;
		}

		playerController.PlacementController.SelectCreature(selectedUnit);
	}

	private void ConfirmPlacement()
	{
		if (hasConfirmedPlacement)
		{
			return;
		}

		hasConfirmedPlacement = true;
		PlacementConfirmed?.Invoke();
	}
}
