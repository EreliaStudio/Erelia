using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerTurnPhaseController : BattlePhaseController, IBattlePhaseInputHandler, IBattlePhaseAbilityShortcutHandler
{
	[SerializeField] private ActionShortcutBarView actionShortcutBar;
	[SerializeField] private ActiveUnitHudView activeUnitHud;

	private PlayerTurnPhase playerTurnPhase;
	private Ability selectedAbility;

	public override BattlePhaseType PhaseType => BattlePhaseType.PlayerTurn;

	protected override void OnStart()
	{
		SubscribeActionShortcutBar();
		SubscribeActiveUnitHud();
	}

	protected override void OnBind()
	{
		if (Orchestrator.TryGetPhase(BattlePhaseType.PlayerTurn, out IBattlePhase phase))
		{
			playerTurnPhase = phase as PlayerTurnPhase;
		}
	}

	protected override void OnActivate()
	{
		actionShortcutBar?.Bind(TurnContext?.ActiveUnit);
		activeUnitHud?.Bind(TurnContext?.ActiveUnit);
		RefreshPreviewOverlay();
	}

	protected override void OnDeactivate()
	{
		selectedAbility = null;
		ClearPreviewOverlay();
		actionShortcutBar?.Bind(null);
		activeUnitHud?.Bind(null);
	}

	public void Confirm() { }

	public void Cancel()
	{
		if (selectedAbility == null) return;
		selectedAbility = null;
		RefreshPreviewOverlay();
	}

	public void SelectAbilityShortcut(int shortcutIndex)
	{
		actionShortcutBar?.TrySelectShortcut(shortcutIndex);
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
	}

	private void SubscribeActionShortcutBar()
	{
		if (actionShortcutBar == null) return;
		actionShortcutBar.AbilityClicked += HandleAbilityClicked;
	}

	private void SubscribeActiveUnitHud()
	{
		if (activeUnitHud == null) return;
		activeUnitHud.EndTurnClicked += HandleEndTurnClicked;
	}

	private void HandleEndTurnClicked()
	{
		if (!IsPhaseActive || playerTurnPhase == null) return;
		playerTurnPhase.TrySubmitEndTurn();
	}

	private void HandleAbilityClicked(int abilityIndex, Ability ability)
	{
		if (!IsPhaseActive || ability == null) return;
		selectedAbility = ability;
		RefreshPreviewOverlay();
	}

	private void TryHandleBoardLeftClick()
	{
		if (playerTurnPhase == null || !TryGetHoveredBoardCell(out Vector3Int cell)) return;

		bool submitted = selectedAbility != null
			? playerTurnPhase.TrySubmitAbility(selectedAbility, new[] { cell })
			: playerTurnPhase.TrySubmitMove(cell);

		if (!submitted) return;

		selectedAbility = null;
		ClearPreviewOverlay();
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

	private void RefreshPreviewOverlay()
	{
		if (BattleMode?.BoardPresenter == null) return;

		BoardOverlayState overlayState = BattleMode.BoardPresenter.OverlayState;
		BattleMaskRules.ClearPreviewMasks(overlayState);

		if (playerTurnPhase != null && TurnContext?.ActiveUnit != null)
		{
			if (selectedAbility != null)
			{
				BattleMaskRules.ApplyMask(overlayState, playerTurnPhase.GetAttackRangeMaskCells(selectedAbility), VoxelMask.AttackRange, clearExisting: false);
			}
			else
			{
				BattleMaskRules.ApplyMask(overlayState, playerTurnPhase.GetMovementRangeMaskCells(), VoxelMask.MovementRange, clearExisting: false);
			}
		}

		BattleMode.BoardPresenter.RefreshOverlay();
	}

	private void ClearPreviewOverlay()
	{
		if (BattleMode?.BoardPresenter == null) return;
		BattleMaskRules.ClearPreviewMasks(BattleMode.BoardPresenter.OverlayState);
		BattleMode.BoardPresenter.RefreshOverlay();
	}
}
