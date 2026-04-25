#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EditorUiFactoryUtility
{
	public static GameObject CreateUiObject<TComponent>(string objectName, MenuCommand menuCommand, Vector2 size)
		where TComponent : Component
	{
		GameObject parent = menuCommand.context as GameObject;
		GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(TComponent));

		GameObject canvas = GetOrCreateCanvas(parent);
		if (parent == null || parent.GetComponentInParent<Canvas>() == null)
		{
			parent = canvas;
		}

		GameObjectUtility.SetParentAndAlign(uiObject, parent);
		Undo.RegisterCreatedObjectUndo(uiObject, $"Create {objectName}");
		StageUtility.PlaceGameObjectInCurrentStage(uiObject);

		RectTransform rectTransform = uiObject.GetComponent<RectTransform>();
		rectTransform.sizeDelta = size;
		rectTransform.localScale = Vector3.one;

		return uiObject;
	}

	public static void SelectAndMarkDirty(GameObject gameObject)
	{
		Selection.activeGameObject = gameObject;
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	private static GameObject GetOrCreateCanvas(GameObject parent)
	{
		Canvas canvas = parent != null ? parent.GetComponentInParent<Canvas>() : Object.FindFirstObjectByType<Canvas>();
		if (canvas != null)
		{
			return canvas.gameObject;
		}

		GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
		StageUtility.PlaceGameObjectInCurrentStage(canvasObject);

		canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

		CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920f, 1080f);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 0.5f;

		CreateEventSystemIfMissing();
		return canvasObject;
	}

	private static void CreateEventSystemIfMissing()
	{
		if (Object.FindFirstObjectByType<EventSystem>() != null)
		{
			return;
		}

		GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
		Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
		StageUtility.PlaceGameObjectInCurrentStage(eventSystemObject);
	}
}
#endif
