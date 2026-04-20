#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CreatureTeamEditorGui
{
	public static void DrawUnitTab(Rect rect, CreatureUnit unit, bool isSelected, System.Action onClick)
	{
		Color backgroundColor = isSelected
			? new Color(0.83f, 0.70f, 0.26f, 1f)
			: (EditorGUIUtility.isProSkin ? new Color(0.20f, 0.20f, 0.20f) : new Color(0.96f, 0.96f, 0.96f));

		EditorGUI.DrawRect(rect, backgroundColor);
		DrawOutline(rect, new Color(0f, 0f, 0f, 0.55f), isSelected ? 2f : 1f);

		const float padding = 6f;
		float iconSize = Mathf.Max(16f, rect.height - padding * 2f);
		Rect iconRect = new Rect(rect.x + padding, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
		Rect labelRect = new Rect(iconRect.xMax + padding, rect.y + padding, rect.xMax - iconRect.xMax - padding * 2f, rect.height - padding * 2f);

		DrawUnitIcon(iconRect, unit);
		GUI.Label(labelRect, EncounterEditorUtility.GetUnitDisplayName(unit), GetTabLabelStyle());

		if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
		{
			onClick?.Invoke();
		}
	}

	public static void DrawUnitIcon(Rect rect, CreatureUnit unit)
	{
		EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.18f));

		Sprite sprite = null;
		try
		{
			CreatureForm form = unit?.GetForm();
			if (form != null)
			{
				sprite = form.Icon;
			}
		}
		catch
		{
		}

		if (sprite != null)
		{
			SpriteGuiUtility.DrawSprite(rect, sprite);
		}
	}

	public static void DrawHeader(CreatureUnit unit, int unitIndex)
	{
		GUILayout.BeginHorizontal();

		Rect iconRect = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
		DrawUnitIcon(iconRect, unit);

		GUILayout.BeginVertical();
		EditorGUILayout.LabelField($"Slot {unitIndex + 1}", EditorStyles.miniLabel);
		EditorGUILayout.LabelField(EncounterEditorUtility.GetUnitDisplayName(unit), EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Current Form", string.IsNullOrEmpty(unit?.CurrentFormID) ? "None" : unit.CurrentFormID);
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
	}

	public static void DrawStats(CreatureUnit unit)
	{
		Attributes attributes = unit?.Attributes ?? new Attributes();
		EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		DrawStatGridRow(("Health", attributes.Health.ToString()), default, default);
		DrawStatGridRow(("AP", attributes.ActionPoints.ToString()), ("MP", attributes.Movement.ToString()), default);
		DrawStatGridRow(
			("Attack", attributes.Attack.ToString()),
			("Armor", attributes.Armor.ToString()),
			("Armor Pen.", attributes.ArmorPenetration.ToString()));
		DrawStatGridRow(
			("Magic", attributes.Magic.ToString()),
			("Resistance", attributes.Resistance.ToString()),
			("Magic Pen.", attributes.ResistancePenetration.ToString()));
		DrawStatGridRow(("Recovery", attributes.Recovery.ToString("0.##")), ("Bonus Healing", attributes.BonusHealing.ToString("0.##")), default);
		DrawStatGridRow(("Life Steal", attributes.LifeSteal.ToString("0.##")), ("Omnivamp", attributes.Omnivamprism.ToString("0.##")), default);
		DrawStatGridRow(("Time Resistance", attributes.TimeEffectResistance.ToString("0.##")), default, default);
		EditorGUILayout.EndVertical();
	}

	public static void DrawObjectList<T>(string title, List<T> items, ref Vector2 scroll) where T : UnityEngine.Object
	{
		EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

		Rect outerRect = GUILayoutUtility.GetRect(0f, 96f, GUILayout.ExpandWidth(true));
		GUI.Box(outerRect, GUIContent.none);

		Rect innerRect = new Rect(outerRect.x + 4f, outerRect.y + 4f, outerRect.width - 8f, outerRect.height - 8f);
		float contentHeight = Mathf.Max(EditorGUIUtility.singleLineHeight, (items?.Count ?? 1) * (EditorGUIUtility.singleLineHeight + 2f));
		Rect contentRect = new Rect(0f, 0f, innerRect.width - 16f, contentHeight);

		scroll = GUI.BeginScrollView(innerRect, scroll, contentRect);

		if (items == null || items.Count == 0)
		{
			GUI.Label(new Rect(0f, 0f, contentRect.width, EditorGUIUtility.singleLineHeight), "None");
		}
		else
		{
			float y = 0f;
			for (int index = 0; index < items.Count; index++)
			{
				T item = items[index];
				GUI.Label(new Rect(0f, y, contentRect.width, EditorGUIUtility.singleLineHeight), "- " + (item != null ? item.name : "None"));
				y += EditorGUIUtility.singleLineHeight + 2f;
			}
		}

		GUI.EndScrollView();
	}

	public static void DrawOutline(Rect rect, Color color, float thickness)
	{
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
		EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
		EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
	}

	private static void DrawStatGridRow((string label, string value) left, (string label, string value) middle, (string label, string value) right)
	{
		const float spacing = 8f;
		Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
		float columnWidth = (rowRect.width - spacing * 2f) / 3f;

		Rect leftRect = new Rect(rowRect.x, rowRect.y, columnWidth, rowRect.height);
		Rect middleRect = new Rect(leftRect.xMax + spacing, rowRect.y, columnWidth, rowRect.height);
		Rect rightRect = new Rect(middleRect.xMax + spacing, rowRect.y, columnWidth, rowRect.height);

		DrawStatCell(leftRect, left.label, left.value);
		DrawStatCell(middleRect, middle.label, middle.value);
		DrawStatCell(rightRect, right.label, right.value);
	}

	private static void DrawStatCell(Rect rect, string label, string value)
	{
		GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
		{
			clipping = TextClipping.Clip
		};

		GUIStyle valueStyle = new GUIStyle(EditorStyles.miniBoldLabel)
		{
			alignment = TextAnchor.MiddleRight,
			clipping = TextClipping.Clip
		};

		const float valueWidth = 36f;
		const float innerSpacing = 4f;
		Rect labelRect = new Rect(rect.x, rect.y, Mathf.Max(0f, rect.width - valueWidth - innerSpacing), rect.height);
		Rect valueRect = new Rect(labelRect.xMax + innerSpacing, rect.y, valueWidth, rect.height);

		EditorGUI.LabelField(labelRect, label ?? string.Empty, labelStyle);
		EditorGUI.LabelField(valueRect, value ?? string.Empty, valueStyle);
	}

	private static GUIStyle GetTabLabelStyle()
	{
		return new GUIStyle(EditorStyles.boldLabel)
		{
			alignment = TextAnchor.MiddleCenter,
			fontSize = 12,
			wordWrap = true,
			clipping = TextClipping.Clip
		};
	}
}
#endif
