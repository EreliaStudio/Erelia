using UnityEditor;
using UnityEngine;

public class EncounterTeamUnitInspectorView
{
	private Vector2 _abilitiesScroll;
	private Vector2 _passivesScroll;
	private Vector2 _inspectorScroll;

	public void Draw(Rect p_rect, SerializedProperty p_unitProperty, CreatureUnit p_unit, EncounterTeamProgressBoardView p_boardView)
	{
		GUILayout.BeginArea(p_rect);

		_inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);

		if (p_unitProperty == null || p_unit == null)
		{
			EditorGUILayout.HelpBox("No unit selected.", MessageType.Info);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
			return;
		}

		SerializedProperty speciesProperty = p_unitProperty.FindPropertyRelative("Species");
		SerializedProperty attributesProperty = p_unitProperty.FindPropertyRelative("Attributes");

		DrawSpeciesRow(speciesProperty, p_unit, p_boardView);

		EditorGUILayout.Space(4f);

		DrawHeader(p_unit);

		EditorGUILayout.Space(6f);
		EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(attributesProperty, GUIContent.none, true);
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.Space(8f);
		DrawScrollableObjectList("Abilities", p_unit.Abilities, ref _abilitiesScroll);

		EditorGUILayout.Space(8f);
		DrawScrollableObjectList("Passives", p_unit.PermanentPassives, ref _passivesScroll);

		EditorGUILayout.Space(12f);
		p_boardView.DrawSelectedNodeActions(p_unit);

		EditorGUILayout.Space(12f);

		if (GUILayout.Button("Edit AI Behaviour", GUILayout.Height(32f)))
		{
			Debug.Log("AI behaviour editor not implemented yet.");
		}

		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawSpeciesRow(SerializedProperty p_speciesProperty, CreatureUnit p_unit, EncounterTeamProgressBoardView p_boardView)
	{
		Rect rect = EditorGUILayout.GetControlRect();

		Rect labelRect = new Rect(
			rect.x,
			rect.y,
			EditorGUIUtility.labelWidth,
			rect.height
		);

		const float buttonWidth = 120f;
		const float spacing = 4f;

		Rect buttonRect = new Rect(
			rect.xMax - buttonWidth,
			rect.y,
			buttonWidth,
			rect.height
		);

		Rect fieldRect = new Rect(
			labelRect.xMax,
			rect.y,
			rect.width - EditorGUIUtility.labelWidth - buttonWidth - spacing,
			rect.height
		);

		EditorGUI.LabelField(labelRect, "Species");

		EditorGUI.BeginChangeCheck();
		CreatureSpecies newSpecies = (CreatureSpecies)EditorGUI.ObjectField(
			fieldRect,
			(CreatureSpecies)p_speciesProperty.objectReferenceValue,
			typeof(CreatureSpecies),
			false
		);
		if (EditorGUI.EndChangeCheck())
		{
			p_speciesProperty.objectReferenceValue = newSpecies;

			if (p_unit != null)
			{
				p_unit.EnsureInitialized();
			}

			if (p_boardView != null)
			{
				p_boardView.ClearSelection();
			}
		}

		CreatureSpecies currentSpecies = (CreatureSpecies)p_speciesProperty.objectReferenceValue;

		using (new EditorGUI.DisabledScope(currentSpecies == null))
		{
			if (GUI.Button(buttonRect, "Edit Feat Board"))
			{
				FeatBoardEditorWindow.Open(currentSpecies);
				GUI.FocusControl(null);
			}
		}
	}

	private void DrawHeader(CreatureUnit p_unit)
	{
		GUILayout.BeginHorizontal();

		Rect iconRect = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
		DrawUnitIcon(iconRect, p_unit);

		GUILayout.BeginVertical();
		EditorGUILayout.LabelField(GetUnitDisplayName(p_unit), EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Form", string.IsNullOrEmpty(p_unit.CurrentFormID) ? "None" : p_unit.CurrentFormID);
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
	}

	private void DrawUnitIcon(Rect p_rect, CreatureUnit p_unit)
	{
		EditorGUI.DrawRect(p_rect, new Color(0f, 0f, 0f, 0.2f));

		Sprite sprite = null;
		try
		{
			CreatureForm form = p_unit.GetForm();
			if (form != null)
			{
				sprite = form.Icon;
			}
		}
		catch
		{
		}

		if (sprite == null || sprite.texture == null)
		{
			return;
		}

		GUI.DrawTexture(p_rect, sprite.texture, ScaleMode.ScaleToFit);
	}

	private string GetUnitDisplayName(CreatureUnit p_unit)
	{
		if (p_unit == null || p_unit.Species == null)
		{
			return "-----";
		}

		if (!string.IsNullOrEmpty(p_unit.CurrentFormID) && p_unit.Species.Forms != null && p_unit.Species.Forms.TryGetValue(p_unit.CurrentFormID, out CreatureForm form) && form != null && !string.IsNullOrEmpty(form.DisplayName))
		{
			return form.DisplayName;
		}

		return p_unit.Species.name;
	}

	private void DrawScrollableObjectList<T>(string p_title, System.Collections.Generic.List<T> p_items, ref Vector2 p_scroll) where T : UnityEngine.Object
	{
		EditorGUILayout.LabelField(p_title, EditorStyles.boldLabel);

		Rect outerRect = GUILayoutUtility.GetRect(0f, 90f, GUILayout.ExpandWidth(true));
		GUI.Box(outerRect, GUIContent.none);

		Rect innerRect = new Rect(outerRect.x + 4f, outerRect.y + 4f, outerRect.width - 8f, outerRect.height - 8f);

		float contentHeight = Mathf.Max(EditorGUIUtility.singleLineHeight, p_items != null ? p_items.Count * (EditorGUIUtility.singleLineHeight + 2f) : EditorGUIUtility.singleLineHeight);
		Rect contentRect = new Rect(0f, 0f, innerRect.width - 16f, contentHeight);

		p_scroll = GUI.BeginScrollView(innerRect, p_scroll, contentRect);

		if (p_items == null || p_items.Count == 0)
		{
			GUI.Label(new Rect(0f, 0f, contentRect.width, EditorGUIUtility.singleLineHeight), "None");
		}
		else
		{
			float y = 0f;

			for (int index = 0; index < p_items.Count; index++)
			{
				T item = p_items[index];
				string label = item != null ? item.name : "None";
				GUI.Label(new Rect(0f, y, contentRect.width, EditorGUIUtility.singleLineHeight), "• " + label);
				y += EditorGUIUtility.singleLineHeight + 2f;
			}
		}

		GUI.EndScrollView();
	}
}