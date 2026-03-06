using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player
{
	/// <summary>
	/// Handles player input for hover/confirm/cancel and forwards confirm/cancel to the active phase.
	/// Tracks the hovered cell and applies selection masks for board interaction.
	/// </summary>
	public sealed class BattlePlayerController : MonoBehaviour
	{
		/// <summary>
		/// Cursor used to detect hovered board cells.
		/// </summary>
		[SerializeField] private Erelia.Battle.Player.Camera.MouseBoardCellCursor cursor;
		/// <summary>
		/// Presenter that owns the battle board model.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		/// <summary>
		/// Active phase controller handling confirm/cancel input.
		/// </summary>
		private Erelia.Battle.PhaseController phaseController;
		/// <summary>
		/// Input action used to confirm placement.
		/// </summary>
		[SerializeField] private InputActionReference confirmAction;
		/// <summary>
		/// Input action used to cancel placement.
		/// </summary>
		[SerializeField] private InputActionReference cancelAction;

		/// <summary>
		/// Resolved input action for confirm.
		/// </summary>
		private InputAction resolvedConfirmAction;
		/// <summary>
		/// Resolved input action for cancel.
		/// </summary>
		private InputAction resolvedCancelAction;
		/// <summary>
		/// Whether the cursor currently hovers a valid cell.
		/// </summary>
		private bool hasHoveredCell;
		/// <summary>
		/// Currently hovered cell coordinate.
		/// </summary>
		private Vector3Int hoveredCell;
		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Validate references and resolve input/team data.
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

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Subscribe to cursor events and enable input.
			if (cursor != null)
			{
				cursor.CellChanged += OnCellChanged;
				cursor.HoverCleared += OnHoverCleared;
			}

			resolvedConfirmAction?.Enable();
			resolvedCancelAction?.Enable();
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Unsubscribe from events and reset selection state.
			if (cursor != null)
			{
				cursor.CellChanged -= OnCellChanged;
				cursor.HoverCleared -= OnHoverCleared;
			}

			resolvedConfirmAction?.Disable();
			resolvedCancelAction?.Disable();
			ClearSelection();
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Poll confirm/cancel input and act accordingly.
			if (IsConfirmTriggered())
			{
				phaseController?.OnConfirm(this);
			}

			if (IsCancelTriggered())
			{
				phaseController?.OnCancel(this);
			}
		}

		/// <summary>
		/// Returns whether a cell is currently hovered.
		/// </summary>
		public bool HasHoveredCell()
		{
			// Expose hover state to callers.
			return hasHoveredCell;
		}

		/// <summary>
		/// Returns the currently hovered cell.
		/// </summary>
		public Vector3Int HoveredCell()
		{
			// Expose the last hovered cell.
			return hoveredCell;
		}

		/// <summary>
		/// Resolves input actions from references.
		/// </summary>
		private void ResolveActions()
		{
			// Resolve confirm and cancel actions.
			if (confirmAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.BattlePlayerController] Confirm action is not assigned.");
			}

			resolvedConfirmAction = confirmAction.action;

			if (cancelAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.BattlePlayerController] Cancel action is not assigned.");
			}

			resolvedCancelAction = cancelAction.action;
		}

		/// <summary>
		/// Handles hover changes from the cursor.
		/// </summary>
		private void OnCellChanged(Vector3Int cell)
		{
			// Update selection mask for the hovered cell.
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

		/// <summary>
		/// Handles hover cleared events.
		/// </summary>
		private void OnHoverCleared()
		{
			// Clear selection when the cursor leaves the board.
			ClearSelection();
		}

		/// <summary>
		/// Clears the current selection mask.
		/// </summary>
		public void ClearSelection()
		{
			// Remove the selection mask and update state.
			if (!hasHoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			hasHoveredCell = false;
			RebuildMasks();
		}

		/// <summary>
		/// Removes the selection mask from the hovered cell.
		/// </summary>
		private void RemoveSelectionMask()
		{
			// Remove the mask from the previous hovered cell.
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

		/// <summary>
		/// Tries to resolve a valid board cell at the given position.
		/// </summary>
		private bool TryResolveBoardCell(Vector3Int cell, out Erelia.Battle.Voxel.Cell resolvedCell)
		{
			// Validate board bounds and return a non-empty cell.
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

		/// <summary>
		/// Rebuilds mask meshes on the board presenter.
		/// </summary>
		private void RebuildMasks()
		{
			// Trigger mask mesh rebuild.
			if (boardPresenter == null)
			{
				return;
			}

			boardPresenter.RebuildMasks();
		}

		/// <summary>
		/// Assigns the active phase controller.
		/// </summary>
		public void SetPhaseController(Erelia.Battle.PhaseController controller)
		{
			// Store the current phase controller.
			phaseController = controller;
		}

		/// <summary>
		/// Checks whether confirm input was triggered this frame.
		/// </summary>
		private bool IsConfirmTriggered()
		{
			// Prefer input action; fallback to mouse click.
			if (resolvedConfirmAction != null)
			{
				return resolvedConfirmAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.leftButton.wasPressedThisFrame;
		}

		/// <summary>
		/// Checks whether cancel input was triggered this frame.
		/// </summary>
		private bool IsCancelTriggered()
		{
			// Prefer input action; fallback to mouse click.
			if (resolvedCancelAction != null)
			{
				return resolvedCancelAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.rightButton.wasPressedThisFrame;
		}
	}
}
