using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlacementPhase : BattlePhase
{
	private readonly BoardPresenter boardPresenter;
	private bool hasConfirmedPlacement;

	public PlacementPhase(BattleContext p_context, BoardPresenter p_boardPresenter) : base(p_context)
	{
		boardPresenter = p_boardPresenter ?? throw new ArgumentNullException(nameof(p_boardPresenter));
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.Placement;

	public event Action PlacementConfirmed;

	public override void Enter()
	{
		hasConfirmedPlacement = false;
		Context.ClearRuntime();
		ResolvePlacementAreas();
		Context.TryPlaceUnitsInCells(Context.EnemyUnits, Context.EnemyPlacementCells);
		Context.TryPlaceUnitsInCells(Context.PlayerUnits, Context.PlayerPlacementCells);
		RenderPlayerPlacementMask();
	}

	public override void Tick(float p_deltaTime)
	{
		if (hasConfirmedPlacement || Keyboard.current == null)
		{
			return;
		}

		if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
		{
			hasConfirmedPlacement = true;
			PlacementConfirmed?.Invoke();
		}
	}

	public override void Exit()
	{
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

		boardPresenter.Rebuild();
	}
}
