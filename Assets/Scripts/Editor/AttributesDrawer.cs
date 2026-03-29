using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Attributes))]
public class AttributesDrawer : PropertyDrawer
{
	private const float RowSpacing = 2f;
	private const float ColumnSpacing = 8f;
	private const float SingleLabelWidth = 95f;
	private const float MiniLabelWidth = 100f;
	private const int RowCount = 6;

	public override void OnGUI(Rect p_position, SerializedProperty p_property, GUIContent p_label)
	{
		EditorGUI.BeginProperty(p_position, p_label, p_property);

		float lineHeight = EditorGUIUtility.singleLineHeight;

		SerializedProperty healthProperty = p_property.FindPropertyRelative("Health");
		SerializedProperty actionPointsProperty = p_property.FindPropertyRelative("ActionPoints");
		SerializedProperty movementProperty = p_property.FindPropertyRelative("Movement");
		SerializedProperty attackProperty = p_property.FindPropertyRelative("Attack");
		SerializedProperty armorProperty = p_property.FindPropertyRelative("Armor");
		SerializedProperty magicProperty = p_property.FindPropertyRelative("Magic");
		SerializedProperty resistanceProperty = p_property.FindPropertyRelative("Resistance");
		SerializedProperty bonusRangeProperty = p_property.FindPropertyRelative("BonusRange");
		SerializedProperty recoveryProperty = p_property.FindPropertyRelative("Recovery");

		Rect currentRect = new Rect(
			p_position.x,
			p_position.y,
			p_position.width,
			lineHeight
		);

		DrawSinglePropertyRow(ref currentRect, "Health", healthProperty);
		DrawDoublePropertyRow(ref currentRect, "Action Points", actionPointsProperty, "Movement", movementProperty);
		DrawDoublePropertyRow(ref currentRect, "Attack", attackProperty, "Armor", armorProperty);
		DrawDoublePropertyRow(ref currentRect, "Magic", magicProperty, "Resistance", resistanceProperty);
		DrawSinglePropertyRow(ref currentRect, "Bonus Range", bonusRangeProperty);
		DrawSinglePropertyRow(ref currentRect, "Recovery", recoveryProperty);

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty p_property, GUIContent p_label)
	{
		return RowCount * EditorGUIUtility.singleLineHeight + (RowCount - 1) * RowSpacing;
	}

	private void DrawSinglePropertyRow(ref Rect p_rowRect, string p_label, SerializedProperty p_property)
	{
		Rect labelRect = new Rect(
			p_rowRect.x,
			p_rowRect.y,
			SingleLabelWidth,
			p_rowRect.height
		);

		Rect valueRect = new Rect(
			labelRect.xMax,
			p_rowRect.y,
			p_rowRect.xMax - labelRect.xMax,
			p_rowRect.height
		);

		EditorGUI.LabelField(labelRect, p_label);
		EditorGUI.PropertyField(valueRect, p_property, GUIContent.none);

		AdvanceRow(ref p_rowRect);
	}

	private void DrawDoublePropertyRow(ref Rect p_rowRect, string p_leftLabel, SerializedProperty p_leftProperty, string p_rightLabel, SerializedProperty p_rightProperty)
	{
		float halfWidth = (p_rowRect.width - ColumnSpacing) * 0.5f;

		Rect leftRect = new Rect(
			p_rowRect.x,
			p_rowRect.y,
			halfWidth,
			p_rowRect.height
		);

		Rect rightRect = new Rect(
			leftRect.xMax + ColumnSpacing,
			p_rowRect.y,
			halfWidth,
			p_rowRect.height
		);

		DrawMiniPropertyRow(leftRect, p_leftLabel, p_leftProperty);
		DrawMiniPropertyRow(rightRect, p_rightLabel, p_rightProperty);
		AdvanceRow(ref p_rowRect);
	}

	private void DrawMiniPropertyRow(Rect p_rect, string p_label, SerializedProperty p_property)
	{
		Rect labelRect = new Rect(
			p_rect.x,
			p_rect.y,
			MiniLabelWidth,
			p_rect.height
		);

		Rect valueRect = new Rect(
			labelRect.xMax,
			p_rect.y,
			p_rect.xMax - labelRect.xMax,
			p_rect.height
		);

		EditorGUI.LabelField(labelRect, p_label);
		EditorGUI.PropertyField(valueRect, p_property, GUIContent.none);
	}

	private void AdvanceRow(ref Rect p_rowRect)
	{
		p_rowRect.y += p_rowRect.height + RowSpacing;
	}
}
