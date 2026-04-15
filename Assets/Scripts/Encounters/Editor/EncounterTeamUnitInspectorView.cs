#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EncounterTeamUnitInspectorView
{
	private Vector2 inspectorScroll;
	private Vector2 abilitiesScroll;
	private Vector2 passivesScroll;

	public void Draw(
		Rect rect,
		EncounterTier.Entry entry,
		int unitIndex,
		EncounterUnit unit,
		EncounterTeamProgressBoardView boardView,
		Action<string, Action> applyChange)
	{
		GUILayout.BeginArea(rect);
		inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

		if (entry == null || unit == null)
		{
			EditorGUILayout.HelpBox("No encounter unit selected.", MessageType.Info);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
			return;
		}

		DrawTeamRow(entry, applyChange);
		EditorGUILayout.Space(6f);
		DrawSpeciesRow(entry, unit, boardView, applyChange);
		EditorGUILayout.Space(6f);
		DrawHeader(unit, unitIndex);
		EditorGUILayout.Space(8f);
		DrawStats(unit);
		EditorGUILayout.Space(8f);
		DrawObjectList("Abilities", unit.Abilities, ref abilitiesScroll);
		EditorGUILayout.Space(8f);
		DrawObjectList("Passives", unit.PermanentPassives, ref passivesScroll);
		EditorGUILayout.Space(12f);
		boardView.DrawSelectedNodeActions(unit, applyChange);
		EditorGUILayout.Space(12f);

		using (new EditorGUI.DisabledScope(true))
		{
			GUILayout.Button("Edit AI Behaviour", GUILayout.Height(32f));
		}

		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawTeamRow(EncounterTier.Entry entry, Action<string, Action> applyChange)
	{
		EditorGUILayout.LabelField("Team", EditorStyles.boldLabel);
		string currentName = entry.DisplayName ?? string.Empty;
		string newName = EditorGUILayout.TextField("Display Name", currentName);
		if (!string.Equals(newName, currentName, StringComparison.Ordinal))
		{
			applyChange?.Invoke("Rename Encounter Team", () => entry.DisplayName = newName);
		}
	}

	private void DrawSpeciesRow(EncounterTier.Entry entry, EncounterUnit unit, EncounterTeamProgressBoardView boardView, Action<string, Action> applyChange)
	{
		Rect rect = EditorGUILayout.GetControlRect();
		Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
		const float buttonWidth = 120f;
		const float spacing = 4f;
		Rect buttonRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);
		Rect fieldRect = new Rect(labelRect.xMax, rect.y, rect.width - EditorGUIUtility.labelWidth - buttonWidth - spacing, rect.height);

		EditorGUI.LabelField(labelRect, "Species");

		CreatureSpecies currentSpecies = unit.Species;
		CreatureSpecies newSpecies = (CreatureSpecies)EditorGUI.ObjectField(fieldRect, currentSpecies, typeof(CreatureSpecies), false);
		if (newSpecies != currentSpecies)
		{
			applyChange?.Invoke("Change Encounter Species", () =>
			{
				unit.Species = newSpecies;
				unit.CurrentFormID = string.Empty;
				unit.FeatBoardProgress = new FeatBoardProgress();
				FeatProgressionService.ApplyProgress(unit);
				boardView?.ClearSelection();

				if (string.IsNullOrWhiteSpace(entry.DisplayName) || entry.DisplayName.StartsWith("team ", StringComparison.OrdinalIgnoreCase))
				{
					entry.DisplayName = EncounterEditorUtility.GetUnitDisplayName(unit);
				}
			});
		}

		using (new EditorGUI.DisabledScope(unit.Species == null))
		{
			if (GUI.Button(buttonRect, "Edit Feat Board"))
			{
				FeatBoardEditorWindow.Open(unit.Species);
				GUI.FocusControl(null);
			}
		}
	}

	private void DrawHeader(EncounterUnit unit, int unitIndex)
	{
		GUILayout.BeginHorizontal();

		Rect iconRect = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
		DrawUnitIcon(iconRect, unit);

		GUILayout.BeginVertical();
		EditorGUILayout.LabelField($"Slot {unitIndex + 1}", EditorStyles.miniLabel);
		EditorGUILayout.LabelField(EncounterEditorUtility.GetUnitDisplayName(unit), EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Current Form", string.IsNullOrEmpty(unit.CurrentFormID) ? "None" : unit.CurrentFormID);
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
	}

	private void DrawUnitIcon(Rect rect, EncounterUnit unit)
	{
		EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.2f));

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

	private void DrawStats(EncounterUnit unit)
	{
		Attributes attributes = unit.Attributes ?? new Attributes();
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

	private void DrawStatGridRow((string label, string value) left, (string label, string value) middle, (string label, string value) right)
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

	private void DrawStatCell(Rect rect, string label, string value)
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

	private void DrawObjectList<T>(string title, List<T> items, ref Vector2 scroll) where T : UnityEngine.Object
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
}
#endif
