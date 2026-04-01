using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class SpriteGuiUtility
{
	public static void DrawSprite(Rect p_rect, Sprite p_sprite)
	{
		if (p_sprite == null || p_sprite.texture == null)
		{
			return;
		}

		Texture2D texture = p_sprite.texture;
		Rect spriteRect = p_sprite.rect;

		Rect uv = new Rect(
			spriteRect.x / texture.width,
			spriteRect.y / texture.height,
			spriteRect.width / texture.width,
			spriteRect.height / texture.height
		);

		GUI.DrawTextureWithTexCoords(p_rect, texture, uv, true);
	}
}

public static class ProgressBarElementUIMenuItems
{
	private const int UI_LAYER = 5;
	private static readonly Vector2 DefaultBarSize = new Vector2(220f, 24f);
	private static readonly Vector2 DefaultCanvasResolution = new Vector2(1920f, 1080f);

	[MenuItem("GameObject/UI/Progress Bar", false, 2031)]
	private static void CreateProgressBar(MenuCommand p_menuCommand)
	{
		GameObject parentObject = ResolveParentObject(p_menuCommand);
		GameObject progressBarObject = new GameObject("Progress Bar", typeof(RectTransform), typeof(CanvasRenderer));
		progressBarObject.layer = UI_LAYER;

		Undo.RegisterCreatedObjectUndo(progressBarObject, "Create Progress Bar");
		GameObjectUtility.SetParentAndAlign(progressBarObject, parentObject);

		RectTransform rectTransform = progressBarObject.GetComponent<RectTransform>();
		rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = DefaultBarSize;

		ProgressBarElementUI progressBarElementUI = Undo.AddComponent<ProgressBarElementUI>(progressBarObject);
		progressBarElementUI.RebuildVisualHierarchy();

		Selection.activeGameObject = progressBarObject;
	}

	private static GameObject ResolveParentObject(MenuCommand p_menuCommand)
	{
		GameObject contextObject = p_menuCommand.context as GameObject;
		if (contextObject != null && contextObject.GetComponentInParent<Canvas>() != null)
		{
			return contextObject;
		}

		Canvas canvas = Object.FindFirstObjectByType<Canvas>();
		if (canvas != null)
		{
			return canvas.gameObject;
		}

		GameObject canvasObject = CreateCanvasObject();
		CreateEventSystemObjectIfNeeded();
		return canvasObject;
	}

	private static GameObject CreateCanvasObject()
	{
		GameObject canvasObject = new GameObject(
			"Canvas",
			typeof(RectTransform),
			typeof(Canvas),
			typeof(CanvasScaler),
			typeof(GraphicRaycaster));
		canvasObject.layer = UI_LAYER;

		Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

		Canvas canvas = canvasObject.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = DefaultCanvasResolution;
		canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		canvasScaler.matchWidthOrHeight = 0.5f;

		RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();
		rectTransform.sizeDelta = DefaultCanvasResolution;

		return canvasObject;
	}

	private static void CreateEventSystemObjectIfNeeded()
	{
		if (Object.FindFirstObjectByType<EventSystem>() != null)
		{
			return;
		}

		GameObject eventSystemObject = new GameObject("EventSystem");
		Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
		eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
		eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
		eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
	}
}
