using System;
using UnityEngine;

public sealed class PlacementPhase : BattlePhase
{
	private readonly BoardPresenter boardPresenter;
	private readonly BattlePlayerController playerController;
	private PlacementAreas areas;

	public PlacementPhase(BattleContext p_context, BoardPresenter p_boardPresenter, BattlePlayerController p_playerController) : base(p_context)
	{
		boardPresenter = p_boardPresenter ?? throw new ArgumentNullException(nameof(p_boardPresenter));
		playerController = p_playerController ?? throw new ArgumentNullException(nameof(p_playerController));
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.Placement;

	public event Action PlacementConfirmed;

	public override void Enter()
	{
		Context.ClearRuntime();
		areas = BattlePlacementInitializer.ResolveAreas(Context.Board);

		playerController.BindPlacement(boardPresenter, Context);
		playerController.PlacementController.HoveredCellChanged += OnHoveredCellChanged;

		RenderPlacementMask();
	}

	public override void Tick(float p_deltaTime)
	{
	}

	public override void Exit()
	{
		playerController.PlacementController.HoveredCellChanged -= OnHoveredCellChanged;
		playerController.UnbindPlacement();
		Context.Board.ClearMask();
		boardPresenter.Rebuild();
		areas = null;
	}

	private void RenderPlacementMask()
	{
		Context.Board.ClearMask();

		for (int index = 0; index < areas.PlayerCells.Count; index++)
		{
			Context.Board.Terrain.MaskLayer.TryAddMask(areas.PlayerCells[index], VoxelMask.Placement);
		}

		boardPresenter.Rebuild();
	}

	private void OnHoveredCellChanged(Vector3Int? p_hoveredCell)
	{
		if (p_hoveredCell.HasValue)
		{
			Logger.LogDebug($"[PlacementPhase] Mouse hovering coordinate {p_hoveredCell.Value}.");
		}
	}
}
