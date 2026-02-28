using UnityEngine;

namespace Erelia.Battle.Player
{
	public sealed class MouseBoardCellSelection : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Player.Camera.MouseBoardCellCursor cursor;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private Erelia.BattleVoxel.Type selectionMask = Erelia.BattleVoxel.Type.Selected;

		private bool hasSelection;
		private Vector3Int selectedCell;

		private void Awake()
		{
			if (cursor == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.MouseBoardCellSelection] Mouse board cell cursor is not assigned.");
			}

			if (boardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.MouseBoardCellSelection] Board presenter is not assigned.");
			}
		}

		private void OnEnable()
		{
			if (cursor != null)
			{
				cursor.CellChanged += OnCellChanged;
				cursor.HoverCleared += OnHoverCleared;
			}

		}

		private void OnDisable()
		{
			if (cursor != null)
			{
				cursor.CellChanged -= OnCellChanged;
				cursor.HoverCleared -= OnHoverCleared;
			}

			ClearSelection();
		}

		public bool TryGetSelectedCell(out Vector3Int cell)
		{
			if (!hasSelection)
			{
				cell = default;
				return false;
			}

			cell = selectedCell;
			return true;
		}

		private void OnCellChanged(Vector3Int cell)
		{
			if (!TryResolveBoardCell(cell, out Erelia.BattleVoxel.Cell targetCell))
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

		private void OnHoverCleared()
		{
			ClearSelection();
		}

		public void ClearSelection()
		{
			if (!hasSelection)
			{
				return;
			}

			RemoveSelectionMask();
			hasSelection = false;
			RebuildMasks();
		}

		private void RemoveSelectionMask()
		{
			if (!hasSelection)
			{
				return;
			}

			if (!TryResolveBoardCell(selectedCell, out Erelia.BattleVoxel.Cell cell))
			{
				return;
			}

			cell.RemoveMask(selectionMask);
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
	}
}
