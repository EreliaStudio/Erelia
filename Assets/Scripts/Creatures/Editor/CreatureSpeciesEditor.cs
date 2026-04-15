using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreatureSpecies))]
public class CreatureSpeciesEditor : Editor
{
	private bool attributesExpanded = true;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawScriptField();
		EditorGUILayout.Space(4f);
		DrawAttributesSection(serializedObject.FindProperty("Attributes"));
		EditorGUILayout.Space(6f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("Forms"), true);
		EditorGUILayout.Space(8f);

		if (GUILayout.Button("Open Feat Board"))
		{
			FeatBoardEditorWindow.Open((CreatureSpecies)target);
		}

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawScriptField()
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((CreatureSpecies)target), typeof(MonoScript), false);
		}
	}

	private void DrawAttributesSection(SerializedProperty attributesProperty)
	{
		if (attributesProperty == null)
		{
			return;
		}

		attributesExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(attributesExpanded, "Attributes");
		if (attributesExpanded)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			DrawAttributeRow(
				("Health", attributesProperty.FindPropertyRelative("Health")),
				default,
				default);
			DrawAttributeRow(
				("AP", attributesProperty.FindPropertyRelative("ActionPoints")),
				("MP", attributesProperty.FindPropertyRelative("Movement")),
				default);
			DrawAttributeRow(
				("Attack", attributesProperty.FindPropertyRelative("Attack")),
				("Armor", attributesProperty.FindPropertyRelative("Armor")),
				("Armor Pen.", attributesProperty.FindPropertyRelative("ArmorPenetration")));
			DrawAttributeRow(
				("Magic", attributesProperty.FindPropertyRelative("Magic")),
				("Resistance", attributesProperty.FindPropertyRelative("Resistance")),
				("Magic Pen.", attributesProperty.FindPropertyRelative("ResistancePenetration")));
			DrawAttributeRow(
				("Recovery", attributesProperty.FindPropertyRelative("Recovery")),
				("Bonus Healing", attributesProperty.FindPropertyRelative("BonusHealing")),
				default);
			DrawAttributeRow(
				("Life Steal", attributesProperty.FindPropertyRelative("LifeSteal")),
				("Omnivamp", attributesProperty.FindPropertyRelative("Omnivamprism")),
				default);
			DrawAttributeRow(
				("Time Resistance", attributesProperty.FindPropertyRelative("TimeEffectResistance")),
				default,
				default);
			EditorGUILayout.EndVertical();
		}

		EditorGUILayout.EndFoldoutHeaderGroup();
	}

	private void DrawAttributeRow((string label, SerializedProperty property) left, (string label, SerializedProperty property) middle, (string label, SerializedProperty property) right)
	{
		const float spacing = 8f;
		Rect rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
		float columnWidth = (rowRect.width - spacing * 2f) / 3f;

		Rect leftRect = new Rect(rowRect.x, rowRect.y, columnWidth, rowRect.height);
		Rect middleRect = new Rect(leftRect.xMax + spacing, rowRect.y, columnWidth, rowRect.height);
		Rect rightRect = new Rect(middleRect.xMax + spacing, rowRect.y, columnWidth, rowRect.height);

		DrawAttributeCell(leftRect, left.label, left.property);
		DrawAttributeCell(middleRect, middle.label, middle.property);
		DrawAttributeCell(rightRect, right.label, right.property);
	}

	private void DrawAttributeCell(Rect rect, string label, SerializedProperty property)
	{
		if (string.IsNullOrEmpty(label) || property == null)
		{
			return;
		}

		const float valueWidth = 44f;
		const float innerSpacing = 4f;

		Rect labelRect = new Rect(rect.x, rect.y, Mathf.Max(0f, rect.width - valueWidth - innerSpacing), rect.height);
		Rect valueRect = new Rect(labelRect.xMax + innerSpacing, rect.y, valueWidth, rect.height);

		EditorGUI.LabelField(labelRect, label, EditorStyles.miniLabel);

		switch (property.propertyType)
		{
			case SerializedPropertyType.Integer:
				property.intValue = EditorGUI.IntField(valueRect, GUIContent.none, property.intValue);
				break;

			case SerializedPropertyType.Float:
				property.floatValue = EditorGUI.FloatField(valueRect, GUIContent.none, property.floatValue);
				break;

			default:
				EditorGUI.PropertyField(valueRect, property, GUIContent.none);
				break;
		}
	}
}
