using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class GameplayInputBlocker
{
	private static readonly List<RaycastResult> UiRaycastResults = new();

	public static bool IsPointerBlockedByUi(Vector2 pointerPosition)
	{
		if (EventSystem.current == null)
		{
			return false;
		}

		PointerEventData pointerEventData = new(EventSystem.current)
		{
			position = pointerPosition
		};

		UiRaycastResults.Clear();
		EventSystem.current.RaycastAll(pointerEventData, UiRaycastResults);
		return UiRaycastResults.Count > 0;
	}

	public static bool TryGetCurrentPointerPosition(out Vector2 pointerPosition)
	{
		if (Pointer.current != null)
		{
			pointerPosition = Pointer.current.position.ReadValue();
			return true;
		}

		pointerPosition = default;
		return false;
	}

	public static bool ShouldBlockPointerAction()
	{
		return TryGetCurrentPointerPosition(out Vector2 pointerPosition) &&
			IsPointerBlockedByUi(pointerPosition);
	}

	public static bool ShouldBlockPointerAction(InputAction.CallbackContext context)
	{
		InputControl control = context.control;
		if (control?.device is not Pointer)
		{
			return false;
		}

		return ShouldBlockPointerAction();
	}
}
