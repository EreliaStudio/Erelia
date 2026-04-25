using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class UiViewUtility
{
	private static Sprite defaultSprite;

	public static GameObject CreateChild(string childName, Transform parent)
	{
		GameObject child = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer));
		child.transform.SetParent(parent, false);
		return child;
	}

	public static void Stretch(RectTransform rectTransform)
	{
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
	}

	public static Sprite GetDefaultSprite()
	{
		if (defaultSprite != null)
		{
			return defaultSprite;
		}

		defaultSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
		defaultSprite.hideFlags = HideFlags.HideAndDontSave;
		return defaultSprite;
	}

	public static void DestroyGeneratedObject(Object target)
	{
		if (target == null)
		{
			return;
		}

#if UNITY_EDITOR
		if (EditorUtility.IsPersistent(target))
		{
			return;
		}
#endif

		if (Application.isPlaying)
		{
			Object.Destroy(target);
		}
		else
		{
			Object.DestroyImmediate(target);
		}
	}

	public static bool IsPersistentAssetContext(MonoBehaviour component)
	{
#if UNITY_EDITOR
		return EditorUtility.IsPersistent(component) ||
			EditorUtility.IsPersistent(component.gameObject) ||
			EditorUtility.IsPersistent(component.transform) ||
			!component.gameObject.scene.IsValid();
#else
		return false;
#endif
	}
}
