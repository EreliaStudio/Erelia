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
		CreatureTeamEditorGui.DrawHeader(unit, unitIndex);
		EditorGUILayout.Space(8f);
		CreatureTeamEditorGui.DrawStats(unit);
		EditorGUILayout.Space(8f);
		CreatureTeamEditorGui.DrawObjectList("Abilities", unit.Abilities, ref abilitiesScroll);
		EditorGUILayout.Space(8f);
		CreatureTeamEditorGui.DrawObjectList("Passives", unit.PermanentPassives, ref passivesScroll);
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
}
#endif
