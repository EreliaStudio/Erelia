#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreatureCardViewFactory
{
	private const string MenuPath = "GameObject/UI/Creature Card";

	[MenuItem(MenuPath, false, 2032)]
	private static void CreateCreatureCard(MenuCommand menuCommand)
	{
		GameObject parent = menuCommand.context as GameObject;
		GameObject creatureCard = new GameObject("Creature Card", typeof(RectTransform), typeof(CreatureCardView));

		GameObject canvas = GetOrCreateCanvas(parent);
		if (parent == null || parent.GetComponentInParent<Canvas>() == null)
		{
			parent = canvas;
		}

		GameObjectUtility.SetParentAndAlign(creatureCard, parent);
		Undo.RegisterCreatedObjectUndo(creatureCard, "Create Creature Card");
		StageUtility.PlaceGameObjectInCurrentStage(creatureCard);

		RectTransform rectTransform = creatureCard.GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(320f, 80f);
		rectTransform.localScale = Vector3.one;

		Selection.activeGameObject = creatureCard;
		EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreateCreatureCard()
	{
		return true;
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

		Canvas createdCanvas = canvasObject.GetComponent<Canvas>();
		createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

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
