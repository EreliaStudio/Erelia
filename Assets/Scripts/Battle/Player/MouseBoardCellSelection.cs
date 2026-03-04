using UnityEngine;

namespace Erelia.Battle.Player
{
	/// <summary>
	/// Tracks mouse hover selection on the battle board.
	/// Applies a selection mask to the hovered cell and clears it when hover ends.
	/// </summary>
	public sealed class MouseBoardCellSelection : MonoBehaviour
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
		/// Mask type applied to the selected cell.
		/// </summary>
		[SerializeField] private Erelia.Battle.Voxel.Type selectionMask = Erelia.Battle.Voxel.Type.Selected;

		/// <summary>
		/// Whether a cell is currently selected.
		/// </summary>
		private bool hasSelection;
		/// <summary>
		/// Currently selected cell coordinate.
		/// </summary>
		private Vector3Int selectedCell;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Validate required references.
			if (cursor == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.MouseBoardCellSelection] Mouse board cell cursor is not assigned.");
			}

			if (boardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.MouseBoardCellSelection] Board presenter is not assigned.");
			}
		}

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Subscribe to cursor hover events.
			if (cursor != null)
			{
				cursor.CellChanged += OnCellChanged;
				cursor.HoverCleared += OnHoverCleared;
			}

		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Unsubscribe and clear selection state.
			if (cursor != null)
			{
				cursor.CellChanged -= OnCellChanged;
				cursor.HoverCleared -= OnHoverCleared;
			}

			ClearSelection();
		}

		/// <summary>
		/// Tries to get the currently selected cell.
		/// </summary>
		public bool TryGetSelectedCell(out Vector3Int cell)
		{
			// Return the selection if one exists.
			if (!hasSelection)
			{
				cell = default;
				return false;
			}

			cell = selectedCell;
			return true;
		}

		/// <summary>
		/// Handles cursor cell changes.
		/// </summary>
		private void OnCellChanged(Vector3Int cell)
		{
			// Update selection mask for the hovered cell.
			if (!TryResolveBoardCell(cell, out Erelia.Battle.Voxel.Cell targetCell))
			{
				ClearSelection();
				return;
			}

			if (hasSelection && cell == selectedCell)
			{
				return;
			}

			RemoveSelectionMask();
			targetCell.AddMask(selectionMask);
			selectedCell = cell;
			hasSelection = true;
			RebuildMasks();
		}

		/// <summary>
		/// Handles cursor hover cleared events.
		/// </summary>
		private void OnHoverCleared()
		{
			// Clear selection when hover ends.
			ClearSelection();
		}

		/// <summary>
		/// Clears the current selection mask.
		/// </summary>
		public void ClearSelection()
		{
			// Remove mask and reset state.
			if (!hasSelection)
			{
				return;
			}

			RemoveSelectionMask();
			hasSelection = false;
			RebuildMasks();
		}

		/// <summary>
		/// Removes the selection mask from the selected cell.
		/// </summary>
		private void RemoveSelectionMask()
		{
			// Remove the mask from the selected cell.
			if (!hasSelection)
			{
				return;
			}

			if (!TryResolveBoardCell(selectedCell, out Erelia.Battle.Voxel.Cell cell))
			{
				return;
			}

			cell.RemoveMask(selectionMask);
		}

		/// <summary>
		/// Tries to resolve a valid board cell at the given position.
		/// </summary>
		private bool TryResolveBoardCell(Vector3Int cell, out Erelia.Battle.Voxel.Cell resolvedCell)
		{
			// Validate bounds and cell content.
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
			// Trigger a mask rebuild if possible.
			if (boardPresenter == null)
			{
				return;
			}

			boardPresenter.RebuildMasks();
		}
	}
}
