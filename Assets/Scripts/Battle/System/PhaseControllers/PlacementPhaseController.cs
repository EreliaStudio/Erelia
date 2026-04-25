using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class PlacementPhaseController : BattlePhaseController, IBattlePhaseInputHandler
{
	[SerializeField] private Button confirmButton;

	[Header("Card Colors")]
	[SerializeField] private Color emptySlotColor = new Color(0.12f, 0.12f, 0.12f, 0.90f);
	[SerializeField] private Color unplacedColor = new Color(0.30f, 0.30f, 0.30f, 0.90f);
	[SerializeField] private Color placedColor = new Color(0.50f, 0.50f, 0.50f, 0.90f);
	[SerializeField] private Color selectedColor = new Color(0.80f, 0.50f, 0.10f, 0.90f);

	private PlacementPhase placementPhase;

	public override BattlePhaseType PhaseType => BattlePhaseType.Placement;

	protected override void OnStart()
	{
		SubscribeConfirmButton();
	}

	protected override void OnBind()
	{
		if (Orchestrator.TryGetPhase(BattlePhaseType.Placement, out IBattlePhase phase))
		{
			placementPhase = phase as PlacementPhase;
		}
	}

	protected override void OnActivate()
	{
		SubscribePlayerCardClicks();
		RefreshCardColors();
		RefreshConfirmButton();
		RefreshPlacementOverlay();
	}

	protected override void OnDeactivate()
	{
		UnsubscribePlayerCardClicks();
		ClearPlacementOverlay();
	}

	public void Confirm()
	{
		if (placementPhase != null && placementPhase.CanCompletePlacement())
		{
			placementPhase.TryCompletePlacement();
		}
	}

	public void Cancel()
	{
		placementPhase?.ClearSelectedPlayerUnit();
		RefreshPlacementOverlay();
	}

	private void Update()
	{
		if (Mouse.current == null || !IsPhaseActive || GameplayInputBlocker.ShouldBlockPointerAction())
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

	private void SubscribePlayerCardClicks()
	{
		if (PlayerTeamView == null) return;
		int cardCount = PlayerTeamView.GetCardCount();
		for (int i = 0; i < cardCount; i++)
		{
			CreatureCardView card = PlayerTeamView.GetCard(i);
			if (card == null) continue;
			card.LeftClicked -= HandlePlayerCardLeftClicked;
			card.RightClicked -= HandlePlayerCardRightClicked;
			card.LeftClicked += HandlePlayerCardLeftClicked;
			card.RightClicked += HandlePlayerCardRightClicked;
		}
	}

	private void UnsubscribePlayerCardClicks()
	{
		if (PlayerTeamView == null) return;
		int cardCount = PlayerTeamView.GetCardCount();
		for (int i = 0; i < cardCount; i++)
		{
			CreatureCardView card = PlayerTeamView.GetCard(i);
			if (card == null) continue;
			card.LeftClicked -= HandlePlayerCardLeftClicked;
			card.RightClicked -= HandlePlayerCardRightClicked;
		}
	}

	private void HandlePlayerCardLeftClicked(BattleUnit unit)
	{
		if (!IsPhaseActive) return;
		placementPhase?.TrySelectPlayerUnit(unit);
		RefreshPlacementOverlay();
	}

	private void HandlePlayerCardRightClicked(BattleUnit unit)
	{
		if (!IsPhaseActive || placementPhase == null || unit == null) return;

		if (!placementPhase.TryRemovePlayerUnit(unit))
		{
			placementPhase.TrySelectPlayerUnit(unit);
		}

		RefreshPlacementOverlay();
	}

	private void TryHandleBoardLeftClick()
	{
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
		if (placementPhase == null || !TryGetHoveredBoardCell(out Vector3Int cell)) return;

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

	private void RefreshCardColors()
	{
		if (BattleContext == null) return;

		BattleUnit selectedUnit = placementPhase?.GetSelectedPlayerUnit();
		RefreshTeamCardColors(PlayerTeamView, BattleContext.PlayerUnits, selectedUnit);
		RefreshTeamCardColors(EnemyTeamView, BattleContext.EnemyUnits, null);
	}

	private void RefreshTeamCardColors(CreatureTeamView teamView, System.Collections.Generic.IReadOnlyList<BattleUnit> units, BattleUnit selectedUnit)
	{
		if (teamView == null) return;
		int cardCount = teamView.GetCardCount();
		for (int i = 0; i < cardCount; i++)
		{
			CreatureCardView card = teamView.GetCard(i);
			if (card == null) continue;
			BattleUnit unit = units != null && i < units.Count ? units[i] : null;
			card.SetBackgroundColor(GetCardColor(unit, selectedUnit));
		}
	}

	private Color GetCardColor(BattleUnit unit, BattleUnit selectedUnit)
	{
		if (unit == null) return emptySlotColor;
		if (ReferenceEquals(unit, selectedUnit)) return selectedColor;
		if (unit.HasBoardPosition) return placedColor;
		return unplacedColor;
	}

	private void RefreshPlacementOverlay()
	{
		if (BattleMode?.BoardPresenter == null) return;

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
		RefreshCardColors();
		RefreshConfirmButton();
	}

	private void ClearPlacementOverlay()
	{
		if (BattleMode?.BoardPresenter == null) return;

		BoardOverlayState overlayState = BattleMode.BoardPresenter.OverlayState;
		overlayState.Clear(VoxelMask.Placement);
		overlayState.Clear(VoxelMask.Selected);
		BattleMode.BoardPresenter.RefreshOverlay();
	}

	private void RefreshConfirmButton()
	{
		if (confirmButton != null)
		{
			confirmButton.interactable = placementPhase != null && placementPhase.CanCompletePlacement();
		}
	}

	private void SubscribeConfirmButton()
	{
		if (confirmButton != null)
		{
			confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
		}
	}

	private void HandleConfirmButtonClicked()
	{
		if (IsPhaseActive)
		{
			placementPhase?.TryCompletePlacement();
		}
	}

}
