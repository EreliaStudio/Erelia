using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EncounterTeamUnitEditor
{
	private const float CardMinHeight = 420f;
	private const float ListAreaHeight = 90f;
	private const float VerticalSpacing = 6f;

	private readonly int _index;
	private readonly SerializedProperty _unitProperty;

	private Vector2 _abilitiesScroll;
	private Vector2 _passivesScroll;

	public EncounterTeamUnitEditor(int p_index, SerializedProperty p_unitProperty)
	{
		_index = p_index;
		_unitProperty = p_unitProperty;
	}

	public void Draw(float p_width)
	{
		GUILayout.BeginVertical("box", GUILayout.Width(p_width), GUILayout.MinHeight(CardMinHeight), GUILayout.ExpandHeight(true));

		DrawHeader();
		GUILayout.Space(VerticalSpacing);

		DrawSpeciesAndForm();
		GUILayout.Space(VerticalSpacing);

		DrawStats();
		GUILayout.Space(VerticalSpacing);

		DrawScrollableObjectList("Abilities", _unitProperty.FindPropertyRelative("Abilities"), ref _abilitiesScroll);
		GUILayout.Space(VerticalSpacing);

		DrawScrollableObjectList("Passives", _unitProperty.FindPropertyRelative("PermanentPassives"), ref _passivesScroll);

		GUILayout.FlexibleSpace();
		GUILayout.Space(VerticalSpacing);

		DrawButtons();

		GUILayout.EndVertical();
	}

	private void DrawHeader()
	{
		EditorGUILayout.LabelField($"Unit {_index + 1}", EditorStyles.boldLabel);
	}

	private void DrawSpeciesAndForm()
	{
		SerializedProperty speciesProperty = _unitProperty.FindPropertyRelative("Species");
		SerializedProperty formIdProperty = _unitProperty.FindPropertyRelative("CurrentFormID");

		EditorGUILayout.PropertyField(speciesProperty);

		if (speciesProperty.objectReferenceValue != null)
		{
			DrawFormSelector(speciesProperty, formIdProperty);
		}
	}

	private void DrawFormSelector(SerializedProperty p_speciesProperty, SerializedProperty p_formIdProperty)
	{
		CreatureSpecies species = p_speciesProperty.objectReferenceValue as CreatureSpecies;
		if (species == null || species.Forms == null || species.Forms.Count == 0)
		{
			EditorGUILayout.HelpBox("No forms available.", MessageType.Warning);
			return;
		}

		List<string> formIds = new List<string>(species.Forms.Keys);

		int currentIndex = formIds.IndexOf(p_formIdProperty.stringValue);
		if (currentIndex < 0)
		{
			currentIndex = 0;
		}

		int newIndex = EditorGUILayout.Popup("Form", currentIndex, formIds.ToArray());

		if (newIndex >= 0 && newIndex < formIds.Count)
		{
			p_formIdProperty.stringValue = formIds[newIndex];
		}
	}

	private void DrawStats()
	{
		SerializedProperty attributesProperty = _unitProperty.FindPropertyRelative("Attributes");
		if (attributesProperty == null)
		{
			return;
		}

		EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(attributesProperty, GUIContent.none, true);
		EditorGUI.EndDisabledGroup();
	}

	private void DrawScrollableObjectList(string p_title, SerializedProperty p_listProperty, ref Vector2 p_scroll)
	{
		EditorGUILayout.LabelField(p_title, EditorStyles.boldLabel);

		Rect outerRect = GUILayoutUtility.GetRect(0f, ListAreaHeight, GUILayout.ExpandWidth(true));
		GUI.Box(outerRect, GUIContent.none);

		Rect viewRect = new Rect(
			outerRect.x + 4f,
			outerRect.y + 4f,
			outerRect.width - 8f,
			outerRect.height - 8f
		);

		float contentHeight = ComputeListContentHeight(p_listProperty);
		Rect contentRect = new Rect(0f, 0f, viewRect.width - 16f, contentHeight);

		p_scroll = GUI.BeginScrollView(viewRect, p_scroll, contentRect);

		if (p_listProperty == null || p_listProperty.arraySize == 0)
		{
			GUI.Label(new Rect(0f, 0f, contentRect.width, EditorGUIUtility.singleLineHeight), "None");
		}
		else
		{
			float y = 0f;

			for (int index = 0; index < p_listProperty.arraySize; index++)
			{
				SerializedProperty elementProperty = p_listProperty.GetArrayElementAtIndex(index);
				Object reference = elementProperty.objectReferenceValue;

				string displayName = reference != null ? reference.name : $"Element {index}";
				Rect lineRect = new Rect(0f, y, contentRect.width, EditorGUIUtility.singleLineHeight);
				GUI.Label(lineRect, $"• {displayName}");

				y += EditorGUIUtility.singleLineHeight + 2f;
			}
		}

		GUI.EndScrollView();
	}

	private float ComputeListContentHeight(SerializedProperty p_listProperty)
	{
		if (p_listProperty == null || p_listProperty.arraySize == 0)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		return p_listProperty.arraySize * (EditorGUIUtility.singleLineHeight + 2f);
	}

	private void DrawButtons()
	{
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Edit AI"))
		{
			Debug.Log($"Edit AI clicked for unit {_index + 1}.");
		}

		if (GUILayout.Button("Unlock Nodes"))
		{
			Debug.Log($"Unlock Nodes clicked for unit {_index + 1}.");
		}

		EditorGUILayout.EndHorizontal();
	}
}