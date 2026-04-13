using UnityEditor;
using UnityEngine;

public partial class FeatBoardEditorWindow
{
	private void EnsureStyles()
	{
		if (nodeTitleStyle == null)
		{
			nodeTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				wordWrap = true,
				clipping = TextClipping.Clip,
				alignment = TextAnchor.MiddleLeft
			};
		}

		if (nodeBodyStyle == null)
		{
			nodeBodyStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				wordWrap = true,
				clipping = TextClipping.Clip
			};
		}

		if (nodeBadgeStyle == null)
		{
			nodeBadgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
			{
				alignment = TextAnchor.MiddleCenter
			};
		}

		if (canvasHintStyle == null)
		{
			canvasHintStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				wordWrap = true
			};
		}

		if (canvasEmptyTitleStyle == null)
		{
			canvasEmptyTitleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				fontSize = 15,
				alignment = TextAnchor.MiddleLeft
			};
		}

		if (canvasEmptyBodyStyle == null)
		{
			canvasEmptyBodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
			{
				alignment = TextAnchor.UpperLeft
			};
		}

		float zoomT = Mathf.InverseLerp(MinZoom, MaxZoom, zoom);
		nodeTitleStyle.fontSize = Mathf.RoundToInt(Mathf.Lerp(11f, 16f, zoomT));
		nodeBodyStyle.fontSize = Mathf.RoundToInt(Mathf.Lerp(9f, 12f, zoomT));
		nodeBadgeStyle.fontSize = Mathf.RoundToInt(Mathf.Lerp(8f, 10f, zoomT));
	}

	private Color GetCanvasBackgroundColor()
	{
		return EditorGUIUtility.isProSkin ? new Color(0.16f, 0.17f, 0.18f) : new Color(0.89f, 0.9f, 0.92f);
	}

	private Color GetMinorGridColor()
	{
		return EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.045f) : new Color(0f, 0f, 0f, 0.045f);
	}

	private Color GetMajorGridColor()
	{
		return EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.11f) : new Color(0f, 0f, 0f, 0.1f);
	}

	private Color GetNodeColor(FeatNodeKind kind)
	{
		switch (kind)
		{
			case FeatNodeKind.StatsBonus:
				return new Color(0.18f, 0.56f, 0.96f);

			case FeatNodeKind.Ability:
				return new Color(0.9f, 0.33f, 0.3f);

			case FeatNodeKind.Passive:
				return new Color(0.2f, 0.76f, 0.48f);

			case FeatNodeKind.Form:
				return new Color(0.96f, 0.7f, 0.22f);

			default:
				return new Color(0.6f, 0.6f, 0.6f);
		}
	}

	private bool ShouldUseDarkText(Color backgroundColor)
	{
		float luminance =
			backgroundColor.r * 0.299f +
			backgroundColor.g * 0.587f +
			backgroundColor.b * 0.114f;

		return luminance >= 0.6f;
	}

	private Color GetReadableTextColor(Color backgroundColor)
	{
		return ShouldUseDarkText(backgroundColor)
			? new Color(0.14f, 0.14f, 0.14f, 0.98f)
			: new Color(0.96f, 0.96f, 0.96f, 0.98f);
	}

	private GUIStyle CreateColoredStyle(GUIStyle baseStyle, Color textColor)
	{
		GUIStyle style = new GUIStyle(baseStyle);
		style.normal.textColor = textColor;
		style.hover.textColor = textColor;
		style.active.textColor = textColor;
		style.focused.textColor = textColor;
		style.onNormal.textColor = textColor;
		style.onHover.textColor = textColor;
		style.onActive.textColor = textColor;
		style.onFocused.textColor = textColor;
		return style;
	}

	private void DrawRectOutline(Rect rect, Color color, float thickness)
	{
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
		EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
	}

	private void DrawSpritePreview(Rect rect, Sprite sprite)
	{
		if (sprite == null || sprite.texture == null)
		{
			return;
		}

		Texture2D texture = sprite.texture;
		Rect spriteRect = sprite.rect;
		Rect uv = new Rect(
			spriteRect.x / texture.width,
			spriteRect.y / texture.height,
			spriteRect.width / texture.width,
			spriteRect.height / texture.height);

		GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
	}

	private Rect ToScreenRect(Rect localRect)
	{
		return new Rect(localRect.x, localRect.y + ToolbarHeight, localRect.width, localRect.height);
	}
}
