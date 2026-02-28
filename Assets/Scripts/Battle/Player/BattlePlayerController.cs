using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player
{
	public sealed class BattlePlayerController : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Player.Camera.MouseBoardCellCursor cursor;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private InputActionReference confirmAction;
		[SerializeField] private InputActionReference cancelAction;

		private InputAction resolvedConfirmAction;
		private InputAction resolvedCancelAction;
		private bool hasHoveredCell;
		private Vector3Int hoveredCell;
		private const Erelia.BattleVoxel.Type PlacementMask = Erelia.BattleVoxel.Type.Placement;

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
			ClearSelection();
		}

		private void Update()
		{
			if (IsConfirmTriggered())
			{
				HandleConfirm();
			}

			if (IsCancelTriggered())
			{
				HandleCancel();
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

		private void ResolveActions()
		{
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

		private void HandleConfirm()
		{
			if (!hasHoveredCell)
			{
				return;
			}

			if (!TryResolveBoardCell(hoveredCell, out Erelia.BattleVoxel.Cell cell))
			{
				return;
			}

			if (!cell.HasMask(PlacementMask))
			{
				Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Confirm ignored. Cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z} is not a valid placement tile.");
				return;
			}

			Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Player wants to place something at cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z}.");
		}

		private void HandleCancel()
		{
			if (hasHoveredCell)
			{
				Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Player cancelled action at cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z}.");
			}

			ClearSelection();
		}

		private void OnCellChanged(Vector3Int cell)
		{
			if (!TryResolveBoardCell(cell, out Erelia.BattleVoxel.Cell targetCell))
			{
				ClearSelection();
				return;
			}

			if (hasHoveredCell && cell == hoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			targetCell.AddMask(Erelia.BattleVoxel.Type.Selected);
			hoveredCell = cell;
			hasHoveredCell = true;
			RebuildMasks();
		}

		private void OnHoverCleared()
		{
			ClearSelection();
		}

		private void ClearSelection()
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

			if (!TryResolveBoardCell(hoveredCell, out Erelia.BattleVoxel.Cell cell))
			{
				return;
			}

			cell.RemoveMask(Erelia.BattleVoxel.Type.Selected);
		}

		private bool TryResolveBoardCell(Vector3Int cell, out Erelia.BattleVoxel.Cell resolvedCell)
		{
			resolvedCell = null;

			Erelia.Battle.Board.Model board = boardPresenter != null ? boardPresenter.Model : null;
			Erelia.BattleVoxel.Cell[,,] cells = board != null ? board.Cells : null;
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

		private bool IsConfirmTriggered()
		{
			if (resolvedConfirmAction != null)
			{
				return resolvedConfirmAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.leftButton.wasPressedThisFrame;
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
	}
}
