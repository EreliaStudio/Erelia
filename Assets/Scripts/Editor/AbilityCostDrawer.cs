using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AbilityCost))]
public class AbilityCostDrawer : PropertyDrawer
{
	public override void OnGUI(Rect p_position, SerializedProperty p_property, GUIContent p_label)
	{
		EditorGUI.BeginProperty(p_position, p_label, p_property);

		SerializedProperty abilityProperty = p_property.FindPropertyRelative("Ability");
		SerializedProperty movementProperty = p_property.FindPropertyRelative("Movement");

		float labelWidth = EditorGUIUtility.labelWidth;
		float spacing = 4f;

		Rect mainLabelRect = new Rect(
			p_position.x,
			p_position.y,
			labelWidth - 4f,
			p_position.height
		);

		float contentX = p_position.x + labelWidth;
		float contentWidth = p_position.width - labelWidth;

		float subLabelWidth = 45f;
		float valueWidth = (contentWidth - subLabelWidth * 2f - spacing * 3f) * 0.5f;

		Rect abilityLabelRect = new Rect(
			contentX,
			p_position.y,
			subLabelWidth,
			p_position.height
		);

		Rect abilityValueRect = new Rect(
			abilityLabelRect.xMax + spacing,
			p_position.y,
			valueWidth,
			p_position.height
		);

		Rect movementLabelRect = new Rect(
			abilityValueRect.xMax + spacing,
			p_position.y,
			subLabelWidth,
			p_position.height
		);

		Rect movementValueRect = new Rect(
			movementLabelRect.xMax + spacing,
			p_position.y,
			valueWidth,
			p_position.height
		);

		EditorGUI.LabelField(mainLabelRect, p_label);
		EditorGUI.LabelField(abilityLabelRect, "AP");
		EditorGUI.PropertyField(abilityValueRect, abilityProperty, GUIContent.none);
		EditorGUI.LabelField(movementLabelRect, "MP");
		EditorGUI.PropertyField(movementValueRect, movementProperty, GUIContent.none);

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty p_property, GUIContent p_label)
	{
		return EditorGUIUtility.singleLineHeight;
	}
}