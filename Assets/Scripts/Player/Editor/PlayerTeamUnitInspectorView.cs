#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class PlayerTeamUnitInspectorView
{
	private Vector2 inspectorScroll;
	private Vector2 abilitiesScroll;
	private Vector2 passivesScroll;

	public void Draw(
		Rect rect,
		SerializedProperty teamProperty,
		int unitIndex,
		CreatureUnit unit,
		EncounterTeamProgressBoardView boardView,
		Action<string, Action> applyChange)
	{
		GUILayout.BeginArea(rect);
		inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

		if (teamProperty == null || unit == null)
		{
			EditorGUILayout.HelpBox("No player creature selected.", MessageType.Info);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
			return;
		}

		DrawSpeciesRow(teamProperty, unitIndex, unit, boardView, applyChange);
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

		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawSpeciesRow(SerializedProperty teamProperty, int unitIndex, CreatureUnit unit, EncounterTeamProgressBoardView boardView, Action<string, Action> applyChange)
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
			applyChange?.Invoke("Change Player Creature Species", () =>
			{
				CreatureUnit currentUnit = GetUnit(teamProperty, unitIndex, true);
				if (currentUnit == null)
				{
					return;
				}

				currentUnit.Species = newSpecies;
				currentUnit.CurrentFormID = string.Empty;
				currentUnit.FeatBoardProgress = new FeatBoardProgress();
				FeatProgressionService.ApplyProgress(currentUnit);
				boardView?.ClearSelection();
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

	private void DrawHeader(CreatureUnit unit, int unitIndex)
	{
		CreatureTeamEditorGui.DrawHeader(unit, unitIndex);
	}

	private void DrawStats(CreatureUnit unit)
	{
		CreatureTeamEditorGui.DrawStats(unit);
	}

	private void DrawObjectList<T>(string title, List<T> items, ref Vector2 scroll) where T : UnityEngine.Object
	{
		CreatureTeamEditorGui.DrawObjectList(title, items, ref scroll);
	}

	private static CreatureUnit GetUnit(SerializedProperty teamProperty, int unitIndex, bool createIfMissing)
	{
		if (teamProperty == null || !teamProperty.isArray || unitIndex < 0 || unitIndex >= teamProperty.arraySize)
		{
			return null;
		}

		SerializedProperty elementProperty = teamProperty.GetArrayElementAtIndex(unitIndex);
		if (elementProperty == null)
		{
			return null;
		}

		if (createIfMissing && elementProperty.managedReferenceValue == null)
		{
			elementProperty.managedReferenceValue = new CreatureUnit();
			teamProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

		return elementProperty.managedReferenceValue as CreatureUnit;
	}
}
#endif
