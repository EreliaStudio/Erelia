using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class PlacementPhaseController : BattlePhaseController
{
	[SerializeField] private GameObject placementHudRoot;
	[SerializeField] private CreatureTeamView playerTeamView;
	[SerializeField] private CreatureTeamView enemyTeamView;

	[SerializeField] private Button confirmButton;

	[Header("Card Colors")]
	[SerializeField] private Color emptySlotColor = new Color(0.12f, 0.12f, 0.12f, 0.90f);
	[SerializeField] private Color unplacedColor = new Color(0.30f, 0.30f, 0.30f, 0.90f);
	[SerializeField] private Color placedColor = new Color(0.50f, 0.50f, 0.50f, 0.90f);
	[SerializeField] private Color selectedColor = new Color(0.80f, 0.50f, 0.10f, 0.90f);

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
			SubscribeConfirmButton();
			RefreshCardColors();
			RefreshConfirmButton();
			RefreshPlacementOverlay();
			return;
		}

		UnsubscribePlayerCardClicks();
		UnsubscribeConfirmButton();
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

	private void RefreshCardColors()
	{
		if (BattleContext == null)
		{
			return;
		}

		BattleUnit selectedUnit = placementPhase?.GetSelectedPlayerUnit();
		RefreshTeamCardColors(playerTeamView, BattleContext.PlayerUnits, selectedUnit);
		RefreshTeamCardColors(enemyTeamView, BattleContext.EnemyUnits, null);
	}

	private void RefreshTeamCardColors(CreatureTeamView teamView, IReadOnlyList<BattleUnit> units, BattleUnit selectedUnit)
	{
		if (teamView == null)
		{
			return;
		}

		int cardCount = teamView.GetCardCount();
		for (int index = 0; index < cardCount; index++)
		{
			CreatureCardView card = teamView.GetCard(index);
			if (card == null)
			{
				continue;
			}

			BattleUnit unit = units != null && index < units.Count ? units[index] : null;
			card.SetBackgroundColor(GetCardBackgroundColor(unit, selectedUnit));
		}
	}

	private Color GetCardBackgroundColor(BattleUnit unit, BattleUnit selectedUnit)
	{
		if (unit == null) return emptySlotColor;
		if (ReferenceEquals(unit, selectedUnit)) return selectedColor;
		if (unit.HasBoardPosition) return placedColor;
		return unplacedColor;
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
		RefreshCardColors();
		RefreshConfirmButton();
	}

	private void RefreshConfirmButton()
	{
		if (confirmButton == null)
		{
			return;
		}

		confirmButton.interactable = placementPhase != null && placementPhase.CanCompletePlacement();
	}

	private void SubscribeConfirmButton()
	{
		if (confirmButton == null)
		{
			return;
		}

		confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
		confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
	}

	private void UnsubscribeConfirmButton()
	{
		if (confirmButton == null)
		{
			return;
		}

		confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
	}

	private void HandleConfirmButtonClicked()
	{
		ResolvePlacementPhase();
		placementPhase?.TryCompletePlacement();
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
