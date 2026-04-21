using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BattlePlacementController
{
	private const float DefaultRaycastDistance = 512f;
	private const float PointerChangeThreshold = 0.0001f;
	private const float CameraPositionChangeThreshold = 0.000001f;
	private const float CameraRotationChangeThreshold = 0.01f;

	private BoardPresenter boardPresenter;
	private BattleContext battleContext;
	private BattleUnit selectedUnit;
	private Vector3 lastCameraPosition;
	private Quaternion lastCameraRotation;
	private bool hasLastCameraTransform;
	private Vector2 lastPointerPosition;
	private bool hasLastPointerPosition;
	private bool hoverDirty = true;
	private Vector3Int? lastValidHoveredCell;

	public BattleUnit SelectedUnit => selectedUnit;
	public Vector3Int? HoveredCell { get; private set; }

	public event Action<BattleUnit> SelectedUnitChanged;
	public event Action<Vector3Int?> HoveredCellChanged;
	public event Action<BattleUnit, Vector3Int> PlacementRequested;
	public event Action<Vector3Int> UnplaceRequested;
	public event Action ConfirmRequested;

	public void Bind(BoardPresenter p_boardPresenter, BattleContext p_battleContext)
	{
		boardPresenter = p_boardPresenter;
		battleContext = p_battleContext;
		hoverDirty = true;
		hasLastCameraTransform = false;
		hasLastPointerPosition = false;
		lastValidHoveredCell = null;
		SetHoveredCell(null);
	}

	public void Unbind()
	{
		boardPresenter = null;
		battleContext = null;
		selectedUnit = null;
		hoverDirty = true;
		hasLastCameraTransform = false;
		hasLastPointerPosition = false;
		lastValidHoveredCell = null;
		SetHoveredCell(null);
	}

	public void SelectCreature(BattleUnit p_unit)
	{
		if (selectedUnit == p_unit)
		{
			return;
		}

		selectedUnit = p_unit;
		hoverDirty = true;
		SelectedUnitChanged?.Invoke(selectedUnit);
	}

	public void Tick(Camera p_camera)
	{
		if (boardPresenter == null || battleContext == null || p_camera == null)
		{
			SetHoveredCell(null);
			return;
		}

		UpdateHoveredCell(p_camera);

		if (HoveredCell.HasValue)
		{
			lastValidHoveredCell = HoveredCell;
		}

		Vector3Int? clickTarget = lastValidHoveredCell;
		if (clickTarget.HasValue)
		{
			if (selectedUnit != null && WasLeftClickRequested())
			{
				PlacementRequested?.Invoke(selectedUnit, clickTarget.Value);
			}

			if (WasRightClickRequested())
			{
				UnplaceRequested?.Invoke(clickTarget.Value);
			}
		}

		if (WasConfirmRequested())
		{
			ConfirmRequested?.Invoke();
		}
	}

	private void UpdateHoveredCell(Camera p_camera)
	{
		if (!TryGetPointerPosition(out Vector2 pointerPosition))
		{
			SetHoveredCell(null);
			hasLastCameraTransform = false;
			hasLastPointerPosition = false;
			hoverDirty = true;
			return;
		}

		bool cameraChanged = !hasLastCameraTransform ||
		                     (p_camera.transform.position - lastCameraPosition).sqrMagnitude > CameraPositionChangeThreshold ||
		                     Quaternion.Angle(p_camera.transform.rotation, lastCameraRotation) > CameraRotationChangeThreshold;
		bool pointerChanged = !hasLastPointerPosition || (pointerPosition - lastPointerPosition).sqrMagnitude > PointerChangeThreshold;

		if (!hoverDirty && !pointerChanged && !cameraChanged)
		{
			return;
		}

		lastPointerPosition = pointerPosition;
		hasLastPointerPosition = true;
		lastCameraPosition = p_camera.transform.position;
		lastCameraRotation = p_camera.transform.rotation;
		hasLastCameraTransform = true;
		hoverDirty = false;

		Ray ray = p_camera.ScreenPointToRay(pointerPosition);
		if (TryResolveBoardCell(ray, out Vector3Int hoveredCell))
		{
			SetHoveredCell(hoveredCell);
			return;
		}

		SetHoveredCell(null);
	}

	private bool TryResolveBoardCell(Ray p_ray, out Vector3Int p_cell)
	{
		p_cell = default;
		if (boardPresenter?.BoardData == null)
		{
			return false;
		}

		if (!BoardVoxelRaycaster.TryRaycast(boardPresenter, p_ray, DefaultRaycastDistance, out BoardVoxelRaycaster.Hit hit))
		{
			return false;
		}

		return BoardPathfinder.TryResolveSelectableTarget(boardPresenter.BoardData, hit.LocalPosition, out p_cell);
	}

	private void SetHoveredCell(Vector3Int? p_hoveredCell)
	{
		if (HoveredCell == p_hoveredCell)
		{
			return;
		}

		HoveredCell = p_hoveredCell;
		HoveredCellChanged?.Invoke(HoveredCell);
	}

	private static bool WasLeftClickRequested()
	{
		return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
	}

	private static bool WasRightClickRequested()
	{
		return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
	}

	private static bool WasConfirmRequested()
	{
		return Keyboard.current != null &&
		       (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
	}

	private static bool TryGetPointerPosition(out Vector2 p_pointerPosition)
	{
		if (Mouse.current == null)
		{
			p_pointerPosition = default;
			return false;
		}

		p_pointerPosition = Mouse.current.position.ReadValue();
		return true;
	}
}
