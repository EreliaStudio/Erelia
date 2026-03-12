using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player
{
	/// <summary>
	/// Handles player input for hover/confirm/cancel and exposes action shortcut selection events.
	/// Tracks the hovered cell and applies selection masks for board interaction.
	/// </summary>
	public sealed class BattlePlayerController : MonoBehaviour
	{
		private const int MaxActionShortcutCount = Erelia.Core.Creature.Instance.Model.MaxAttackCount;

		[SerializeField] private Erelia.Battle.Player.Camera.MouseBoardCellCursor cursor;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private InputActionReference confirmAction;
		[SerializeField] private InputActionReference cancelAction;
		[SerializeField] private InputActionReference[] actionShortcutActions =
			new InputActionReference[MaxActionShortcutCount];

		private Erelia.Battle.Phase.Controller phaseController;
		private InputAction resolvedConfirmAction;
		private InputAction resolvedCancelAction;
		private readonly InputAction[] resolvedActionShortcutActions = new InputAction[MaxActionShortcutCount];
		private readonly Action<InputAction.CallbackContext>[] actionShortcutCallbacks =
			new Action<InputAction.CallbackContext>[MaxActionShortcutCount];
		private bool hasHoveredCell;
		private Vector3Int hoveredCell;

		public event Action<int> ActionSelected;

		private void Awake()
		{
			if (cursor == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] Mouse board cell cursor is not assigned.");
			}

			if (boardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] Board presenter is not assigned.");
			}

			ResolveActions();
		}

		private void OnEnable()
		{
			if (cursor != null)
			{
				cursor.CellChanged += OnCellChanged;
				cursor.HoverCleared += OnHoverCleared;
			}

			resolvedConfirmAction?.Enable();
			resolvedCancelAction?.Enable();
			BindActionShortcuts();
		}

		private void OnDisable()
		{
			if (cursor != null)
			{
				cursor.CellChanged -= OnCellChanged;
				cursor.HoverCleared -= OnHoverCleared;
			}

			resolvedConfirmAction?.Disable();
			resolvedCancelAction?.Disable();
			UnbindActionShortcuts();
			ClearSelection();
		}

		private void Update()
		{
			if (IsConfirmTriggered())
			{
				phaseController?.OnConfirm(this);
			}

			if (IsCancelTriggered())
			{
				phaseController?.OnCancel(this);
			}
		}

		public bool HasHoveredCell()
		{
			return hasHoveredCell;
		}

		public Vector3Int HoveredCell()
		{
			return hoveredCell;
		}

		public void RequestActionShortcut(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= MaxActionShortcutCount)
			{
				return;
			}

			ActionSelected?.Invoke(slotIndex);
		}

		public string GetActionShortcutLabel(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= MaxActionShortcutCount)
			{
				return string.Empty;
			}

			InputAction action = resolvedActionShortcutActions[slotIndex];
			if (action != null && action.bindings.Count > 0)
			{
				string label = action.GetBindingDisplayString(0);
				if (!string.IsNullOrWhiteSpace(label))
				{
					return label;
				}
			}

			return (slotIndex + 1).ToString();
		}

		private void ResolveActions()
		{
			if (confirmAction == null || confirmAction.action == null)
			{
				throw new Exception("[Erelia.Battle.Player.BattlePlayerController] Confirm action is not assigned.");
			}

			if (cancelAction == null || cancelAction.action == null)
			{
				throw new Exception("[Erelia.Battle.Player.BattlePlayerController] Cancel action is not assigned.");
			}

			resolvedConfirmAction = confirmAction.action;
			resolvedCancelAction = cancelAction.action;
			ResolveActionShortcuts();
		}

		private void ResolveActionShortcuts()
		{
			InputActionMap actionMap = resolvedConfirmAction != null ? resolvedConfirmAction.actionMap : null;
			for (int i = 0; i < resolvedActionShortcutActions.Length; i++)
			{
				InputActionReference shortcutReference =
					actionShortcutActions != null && i < actionShortcutActions.Length
						? actionShortcutActions[i]
						: null;

				InputAction action = shortcutReference != null ? shortcutReference.action : null;
				if (action == null && actionMap != null)
				{
					action = actionMap.FindAction(BuildActionShortcutName(i), throwIfNotFound: false);
				}

				resolvedActionShortcutActions[i] = action;
			}
		}

		private void BindActionShortcuts()
		{
			for (int i = 0; i < resolvedActionShortcutActions.Length; i++)
			{
				InputAction action = resolvedActionShortcutActions[i];
				if (action == null)
				{
					continue;
				}

				if (actionShortcutCallbacks[i] != null)
				{
					action.performed -= actionShortcutCallbacks[i];
				}

				int slotIndex = i;
				actionShortcutCallbacks[i] = _ => RequestActionShortcut(slotIndex);
				action.performed += actionShortcutCallbacks[i];
				action.Enable();
			}
		}

		private void UnbindActionShortcuts()
		{
			for (int i = 0; i < resolvedActionShortcutActions.Length; i++)
			{
				InputAction action = resolvedActionShortcutActions[i];
				Action<InputAction.CallbackContext> callback = actionShortcutCallbacks[i];
				if (action == null)
				{
					continue;
				}

				if (callback != null)
				{
					action.performed -= callback;
					actionShortcutCallbacks[i] = null;
				}

				action.Disable();
			}
		}

		private void OnCellChanged(Vector3Int cell)
		{
			if (!TryResolveBoardCell(cell, out Erelia.Battle.Voxel.Cell targetCell))
			{
				ClearSelection();
				return;
			}

			if (hasHoveredCell && cell == hoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			targetCell.AddMask(Erelia.Battle.Voxel.Mask.Type.Selected);
			hoveredCell = cell;
			hasHoveredCell = true;
			RebuildMasks();
		}

		private void OnHoverCleared()
		{
			ClearSelection();
		}

		public void ClearSelection()
		{
			if (!hasHoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			hasHoveredCell = false;
			RebuildMasks();
		}

		private void RemoveSelectionMask()
		{
			if (!hasHoveredCell)
			{
				return;
			}

			if (!TryResolveBoardCell(hoveredCell, out Erelia.Battle.Voxel.Cell cell))
			{
				return;
			}

			cell.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Selected);
		}

		private bool TryResolveBoardCell(Vector3Int cell, out Erelia.Battle.Voxel.Cell resolvedCell)
		{
			resolvedCell = null;

			Erelia.Battle.Board.Model board = boardPresenter != null ? boardPresenter.Model : null;
			Erelia.Battle.Voxel.Cell[,,] cells = board != null ? board.Cells : null;
			if (cells == null)
			{
				return false;
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			if (cell.x < 0 || cell.x >= sizeX ||
				cell.y < 0 || cell.y >= sizeY ||
				cell.z < 0 || cell.z >= sizeZ)
			{
				return false;
			}

			resolvedCell = cells[cell.x, cell.y, cell.z];
			if (resolvedCell == null || resolvedCell.Id < 0)
			{
				resolvedCell = null;
				return false;
			}

			return true;
		}

		private void RebuildMasks()
		{
			if (boardPresenter == null)
			{
				return;
			}

			boardPresenter.RebuildMasks();
		}

		public void SetPhaseController(Erelia.Battle.Phase.Controller controller)
		{
			phaseController = controller;
		}

		private bool IsConfirmTriggered()
		{
			if (resolvedConfirmAction != null)
			{
				return resolvedConfirmAction.WasPerformedThisFrame() && !IsPointerOverUi();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.leftButton.wasPressedThisFrame && !IsPointerOverUi();
		}

		private bool IsCancelTriggered()
		{
			if (resolvedCancelAction != null)
			{
				return resolvedCancelAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.rightButton.wasPressedThisFrame;
		}

		private static bool IsPointerOverUi()
		{
			return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		}

		private static string BuildActionShortcutName(int slotIndex)
		{
			return "ActionShortcut" + (slotIndex + 1);
		}
	}
}
