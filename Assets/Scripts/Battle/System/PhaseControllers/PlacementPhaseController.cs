using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlacementPhaseController : BattlePhaseController
{
	[SerializeField] private GameObject placementHudRoot;
	[SerializeField] private CreatureTeamView playerTeamView;
	[SerializeField] private CreatureTeamView enemyTeamView;

	private PlacementPhase placementPhase;

	public override BattlePhaseType PhaseType => BattlePhaseType.Placement;

	private void Update()
	{
		if (Mouse.current == null || GameplayInputBlocker.ShouldBlockPointerAction())
		{
			return;
		}

		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			TryHandleBoardLeftClick();
		}

		if (Mouse.current.rightButton.wasPressedThisFrame)
		{
			TryHandleBoardRightClick();
		}
	}

	public override void SetActive(bool isActive)
	{
		if (placementHudRoot != null)
		{
			placementHudRoot.SetActive(isActive);
		}

		base.SetActive(isActive);

		if (isActive)
		{
			ResolvePlacementPhase();
			BindTeams();
			SubscribePlayerCardClicks();
			RefreshPlacementOverlay();
			return;
		}

		UnsubscribePlayerCardClicks();
		ClearPlacementOverlay();
	}

	protected override void OnBind()
	{
		ResolvePlacementPhase();
		BindTeams();
	}

	protected override void OnConfirmAction(InputAction.CallbackContext context)
	{
		ResolvePlacementPhase();
		if (placementPhase != null && placementPhase.CanCompletePlacement())
		{
			placementPhase.TryCompletePlacement();
		}
	}

	protected override void OnCancelAction(InputAction.CallbackContext context)
	{
		ResolvePlacementPhase();
		placementPhase?.ClearSelectedPlayerUnit();
		RefreshPlacementOverlay();
	}

	private void BindTeams()
	{
		if (BattleContext == null)
		{
			if (playerTeamView != null)
			{
				playerTeamView.Bind(null);
			}

			if (enemyTeamView != null)
			{
				enemyTeamView.Bind(null);
			}

			return;
		}

		playerTeamView?.Bind(BattleContext.PlayerUnits);
		enemyTeamView?.Bind(BattleContext.EnemyUnits);
	}

	private void SubscribePlayerCardClicks()
	{
		if (playerTeamView == null)
		{
			return;
		}

		UnsubscribePlayerCardClicks();

		int cardCount = playerTeamView.GetCardCount();
		for (int index = 0; index < cardCount; index++)
		{
			CreatureCardView card = playerTeamView.GetCard(index);
			card?.AddLeftClickListener(HandlePlayerCardLeftClicked);
			card?.AddRightClickListener(HandlePlayerCardRightClicked);
		}
	}

	private void UnsubscribePlayerCardClicks()
	{
		if (playerTeamView == null)
		{
			return;
		}

		int cardCount = playerTeamView.GetCardCount();
		for (int index = 0; index < cardCount; index++)
		{
			CreatureCardView card = playerTeamView.GetCard(index);
			card?.RemoveLeftClickListener(HandlePlayerCardLeftClicked);
			card?.RemoveRightClickListener(HandlePlayerCardRightClicked);
		}
	}

	private void HandlePlayerCardLeftClicked(BattleUnit unit)
	{
		ResolvePlacementPhase();
		placementPhase?.TrySelectPlayerUnit(unit);
		RefreshPlacementOverlay();
	}

	private void HandlePlayerCardRightClicked(BattleUnit unit)
	{
		ResolvePlacementPhase();
		if (placementPhase == null || unit == null)
		{
			return;
		}

		if (!placementPhase.TryRemovePlayerUnit(unit))
		{
			placementPhase.TrySelectPlayerUnit(unit);
		}

		RefreshPlacementOverlay();
	}

	private void ResolvePlacementPhase()
	{
		if (placementPhase != null)
		{
			return;
		}

		if (Orchestrator != null &&
			Orchestrator.TryGetPhase(BattlePhaseType.Placement, out IBattlePhase phase))
		{
			placementPhase = phase as PlacementPhase;
		}
	}

	private void TryHandleBoardLeftClick()
	{
		ResolvePlacementPhase();
		if (placementPhase == null ||
			placementPhase.GetSelectedPlayerUnit() == null ||
			!TryGetHoveredBoardCell(out Vector3Int cell))
		{
			return;
		}

		if (placementPhase.TryPlaceUnit(placementPhase.GetSelectedPlayerUnit(), cell))
		{
			RefreshPlacementOverlay();
		}
	}

	private void TryHandleBoardRightClick()
	{
		ResolvePlacementPhase();
		if (placementPhase == null || !TryGetHoveredBoardCell(out Vector3Int cell))
		{
			return;
		}

		if (placementPhase.TryRemovePlayerUnitAt(cell))
		{
			RefreshPlacementOverlay();
		}
	}

	private bool TryGetHoveredBoardCell(out Vector3Int cell)
	{
		cell = default;
		if (BattleMode?.BoardPresenter == null ||
			Camera.main == null ||
			!GameplayInputBlocker.TryGetCurrentPointerPosition(out Vector2 pointerPosition))
		{
			return false;
		}

		Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
		if (!BoardVoxelRaycaster.TryRaycast(BattleMode.BoardPresenter, ray, 500f, out BoardVoxelRaycaster.Hit hit))
		{
			return false;
		}

		cell = hit.LocalPosition;
		return true;
	}

	private void RefreshPlacementOverlay()
	{
		if (BattleMode?.BoardPresenter == null)
		{
			return;
		}

		BoardOverlayState overlayState = BattleMode.BoardPresenter.OverlayState;
		overlayState.Clear(VoxelMask.Placement);
		overlayState.Clear(VoxelMask.Selected);

		if (placementPhase != null)
		{
			BattleMaskRules.ApplyMask(overlayState, placementPhase.GetPlacementMaskCells(), VoxelMask.Placement, clearExisting: false);

			BattleUnit selectedUnit = placementPhase.GetSelectedPlayerUnit();
			if (selectedUnit != null && selectedUnit.HasBoardPosition)
			{
				overlayState.ApplyMask(new[] { selectedUnit.BoardPosition }, VoxelMask.Selected);
			}
		}

		BattleMode.BoardPresenter.RefreshOverlay();
	}

	private void ClearPlacementOverlay()
	{
		if (BattleMode?.BoardPresenter == null)
		{
			return;
		}

		BoardOverlayState overlayState = BattleMode.BoardPresenter.OverlayState;
		overlayState.Clear(VoxelMask.Placement);
		overlayState.Clear(VoxelMask.Selected);
		BattleMode.BoardPresenter.RefreshOverlay();
	}
}
