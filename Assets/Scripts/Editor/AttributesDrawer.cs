using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Attributes))]
public class AttributesDrawer : PropertyDrawer
{
	private const float RowSpacing = 2f;

	private const float LeftLabelWidth = 95f;
	private const float LeftValueWidth = 60f;
	private const float RightLabelWidth = 80f;
	private const float ColumnSpacing = 8f;

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

		DrawSingleStatRow(ref currentRect, "Health", healthProperty);
		DrawDoubleStatRow(ref currentRect, "Action Points", actionPointsProperty, "Movement", movementProperty);
		DrawDoubleIntRow(ref currentRect, "Attack", attackProperty, "Armor", armorProperty);
		DrawDoubleIntRow(ref currentRect, "Magic", magicProperty, "Resistance", resistanceProperty);
		DrawSingleIntRow(ref currentRect, "Bonus Range", bonusRangeProperty);
		DrawSingleFloatRow(ref currentRect, "Recovery", recoveryProperty);

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty p_property, GUIContent p_label)
	{
		int rowCount = 6;
		return rowCount * EditorGUIUtility.singleLineHeight + (rowCount - 1) * RowSpacing;
	}

	private void DrawSingleStatRow(ref Rect p_rowRect, string p_label, SerializedProperty p_statProperty)
	{
		SerializedProperty maximumProperty = p_statProperty.FindPropertyRelative("Maximum");
		DrawSinglePropertyRow(ref p_rowRect, p_label, maximumProperty);
	}

	private void DrawSingleIntRow(ref Rect p_rowRect, string p_label, SerializedProperty p_property)
	{
		DrawSinglePropertyRow(ref p_rowRect, p_label, p_property);
	}

	private void DrawSingleFloatRow(ref Rect p_rowRect, string p_label, SerializedProperty p_property)
	{
		DrawSinglePropertyRow(ref p_rowRect, p_label, p_property);
	}

	private void DrawSinglePropertyRow(ref Rect p_rowRect, string p_label, SerializedProperty p_property)
	{
		GetColumnRects(
			p_rowRect,
			out Rect leftLabelRect,
			out Rect leftValueRect,
			out Rect rightLabelRect,
			out Rect rightValueRect
		);

		Rect singleValueRect = new Rect(
			leftValueRect.x,
			leftValueRect.y,
			p_rowRect.xMax - leftValueRect.x,
			leftValueRect.height
		);

		EditorGUI.LabelField(leftLabelRect, p_label);
		EditorGUI.PropertyField(singleValueRect, p_property, GUIContent.none);

		p_rowRect.y += p_rowRect.height + RowSpacing;
	}

	private void DrawDoubleStatRow(ref Rect p_rowRect, string p_leftLabel, SerializedProperty p_leftStatProperty, string p_rightLabel, SerializedProperty p_rightStatProperty)
	{
		SerializedProperty leftMaximumProperty = p_leftStatProperty.FindPropertyRelative("Maximum");
		SerializedProperty rightMaximumProperty = p_rightStatProperty.FindPropertyRelative("Maximum");

		DrawDoublePropertyRow(ref p_rowRect, p_leftLabel, leftMaximumProperty, p_rightLabel, rightMaximumProperty);
	}

	private void DrawDoubleIntRow(ref Rect p_rowRect, string p_leftLabel, SerializedProperty p_leftProperty, string p_rightLabel, SerializedProperty p_rightProperty)
	{
		DrawDoublePropertyRow(ref p_rowRect, p_leftLabel, p_leftProperty, p_rightLabel, p_rightProperty);
	}

	private void DrawDoublePropertyRow(ref Rect p_rowRect, string p_leftLabel, SerializedProperty p_leftProperty, string p_rightLabel, SerializedProperty p_rightProperty)
{
	float totalWidth = p_rowRect.width;
	float spacing = 8f;

	float halfWidth = (totalWidth - spacing) * 0.5f;

	Rect leftRect = new Rect(
		p_rowRect.x,
		p_rowRect.y,
		halfWidth,
		p_rowRect.height
	);

	Rect rightRect = new Rect(
		leftRect.xMax + spacing,
		p_rowRect.y,
		halfWidth,
		p_rowRect.height
	);

	DrawMiniPropertyRow(leftRect, p_leftLabel, p_leftProperty);
	DrawMiniPropertyRow(rightRect, p_rightLabel, p_rightProperty);

	p_rowRect.y += p_rowRect.height + RowSpacing;
}

private void DrawMiniPropertyRow(Rect p_rect, string p_label, SerializedProperty p_property)
{
	const float labelWidth = 100f;

	Rect labelRect = new Rect(
		p_rect.x,
		p_rect.y,
		labelWidth,
		p_rect.height
	);

	Rect valueRect = new Rect(
		labelRect.xMax,
		p_rect.y,
		p_rect.width - labelWidth,
		p_rect.height
	);

	EditorGUI.LabelField(labelRect, p_label);
	EditorGUI.PropertyField(valueRect, p_property, GUIContent.none);
}

	private void GetColumnRects(
		Rect p_rowRect,
		out Rect p_leftLabelRect,
		out Rect p_leftValueRect,
		out Rect p_rightLabelRect,
		out Rect p_rightValueRect)
	{
		float leftLabelX = p_rowRect.x;
		float leftValueX = leftLabelX + LeftLabelWidth;
		float rightLabelX = leftValueX + LeftValueWidth + ColumnSpacing;
		float rightValueX = rightLabelX + RightLabelWidth;

		p_leftLabelRect = new Rect(
			leftLabelX,
			p_rowRect.y,
			LeftLabelWidth,
			p_rowRect.height
		);

		p_leftValueRect = new Rect(
			leftValueX,
			p_rowRect.y,
			LeftValueWidth,
			p_rowRect.height
		);

		p_rightLabelRect = new Rect(
			rightLabelX,
			p_rowRect.y,
			RightLabelWidth,
			p_rowRect.height
		);

		p_rightValueRect = new Rect(
			rightValueX,
			p_rowRect.y,
			p_rowRect.xMax - rightValueX,
			p_rowRect.height
		);
	}
}