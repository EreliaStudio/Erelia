using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Battle.Player.Controller
{
	public class BattleController : MonoBehaviour
	{
		[SerializeField] public Core.Camera.Controller.MouseCellCursorController MouseCellController = null;
	
		private Vector3Int lastHoveredCell;
		private bool hasLastCellHovered = false;

		private void OnEnable()
		{
			gameObject.transform.position = new Vector3(ServiceLocator.Instance.BattleBoardService.Data.SizeX / 2.0f, 1, ServiceLocator.Instance.BattleBoardService.Data.SizeZ / 2.0f);
			MouseCellController.CellHovered += OnCellHovered;
			MouseCellController.HoverCleared += OnHoverCleared;
		}

		private void OnDisable()
		{
			MouseCellController.CellHovered -= OnCellHovered;
			MouseCellController.HoverCleared -= OnHoverCleared;
		}

		void OnCellHovered(Vector3Int cellCoordinates, RaycastHit hit)
		{
			if (hasLastCellHovered == true)
			{
				ServiceLocator.Instance.BattleBoardService.Data.MaskCells[lastHoveredCell.x, lastHoveredCell.y, lastHoveredCell.z].RemoveMask(Core.Mask.Model.Value.Selected);
			}
			lastHoveredCell = cellCoordinates;
			ServiceLocator.Instance.BattleBoardService.Data.MaskCells[lastHoveredCell.x, lastHoveredCell.y, lastHoveredCell.z].AddMask(Core.Mask.Model.Value.Selected);
			hasLastCellHovered = true;
		}

		void OnHoverCleared()
		{
			if (hasLastCellHovered == true)
			{
				ServiceLocator.Instance.BattleBoardService.Data.MaskCells[lastHoveredCell.x, lastHoveredCell.y, lastHoveredCell.z].RemoveMask(Core.Mask.Model.Value.Selected);
				hasLastCellHovered = false;
			}
		}
	}
}