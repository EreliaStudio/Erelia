using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Effect), true)]
public class EffectDrawer : PropertyDrawer
{
	private enum EffectKind
	{
		None,
		DamageTarget,
		HealTarget
	}

	private const float LabelColumnWidth = 90f;
	private const float RowSpacing = 2f;

	public override void OnGUI(Rect p_position, SerializedProperty p_property, GUIContent p_label)
	{
		EditorGUI.BeginProperty(p_position, p_label, p_property);

		float lineHeight = EditorGUIUtility.singleLineHeight;

		Rect currentRect = new Rect(
			p_position.x,
			p_position.y,
			p_position.width,
			lineHeight
		);

		float indentOffset = 15f;
		float labelColumnX = currentRect.x + indentOffset;
		float labelColumnWidth = LabelColumnWidth;
		float valueColumnX = labelColumnX + labelColumnWidth;
		float valueColumnWidth = p_position.xMax - valueColumnX;

		Rect foldoutRect = new Rect(
			currentRect.x,
			currentRect.y,
			valueColumnX - currentRect.x,
			lineHeight
		);

		Rect popupRect = new Rect(
			valueColumnX,
			currentRect.y,
			valueColumnWidth,
			lineHeight
		);

		p_property.isExpanded = EditorGUI.Foldout(
			foldoutRect,
			p_property.isExpanded,
			p_label,
			true
		);

		EffectKind currentKind = GetEffectKindFromProperty(p_property);

		EditorGUI.BeginChangeCheck();
		EffectKind newKind = (EffectKind)EditorGUI.EnumPopup(popupRect, currentKind);
		if (EditorGUI.EndChangeCheck())
		{
			p_property.managedReferenceValue = CreateEffectInstance(newKind);
			p_property.serializedObject.ApplyModifiedProperties();
			p_property.serializedObject.Update();
		}

		if (p_property.isExpanded && p_property.managedReferenceValue != null)
		{
			currentRect.y += lineHeight + RowSpacing;
			DrawChildFields(ref currentRect, p_property, labelColumnX, labelColumnWidth, valueColumnX, valueColumnWidth);
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty p_property, GUIContent p_label)
	{
		float height = EditorGUIUtility.singleLineHeight;

		if (p_property.isExpanded == false)
			return height;

		if (p_property.managedReferenceValue == null)
			return height;

		height += RowSpacing;
		height += GetChildFieldsHeight(p_property);

		return height;
	}

	private void DrawChildFields(
		ref Rect p_currentRect,
		SerializedProperty p_property,
		float p_labelColumnX,
		float p_labelColumnWidth,
		float p_valueColumnX,
		float p_valueColumnWidth)
	{
		if (p_property.managedReferenceValue == null)
			return;

		SerializedProperty iterator = p_property.Copy();
		SerializedProperty endProperty = iterator.GetEndProperty();

		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
		{
			enterChildren = false;

			if (iterator.depth != p_property.depth + 1)
				continue;

			float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);

			Rect labelRect = new Rect(
				p_labelColumnX,
				p_currentRect.y,
				p_labelColumnWidth,
				EditorGUIUtility.singleLineHeight
			);

			Rect valueRect = new Rect(
				p_valueColumnX,
				p_currentRect.y,
				p_valueColumnWidth,
				propertyHeight
			);

			EditorGUI.LabelField(labelRect, iterator.displayName);
			EditorGUI.PropertyField(valueRect, iterator, GUIContent.none, true);

			p_currentRect.y += propertyHeight + RowSpacing;
		}
	}

	private float GetChildFieldsHeight(SerializedProperty p_property)
	{
		if (p_property.managedReferenceValue == null)
			return 0f;

		float totalHeight = 0f;

		SerializedProperty iterator = p_property.Copy();
		SerializedProperty endProperty = iterator.GetEndProperty();

		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
		{
			enterChildren = false;

			if (iterator.depth != p_property.depth + 1)
				continue;

			totalHeight += EditorGUI.GetPropertyHeight(iterator, true);
			totalHeight += RowSpacing;
		}

		return totalHeight;
	}

	private Effect CreateEffectInstance(EffectKind p_kind)
	{
		switch (p_kind)
		{
			case EffectKind.DamageTarget:
				return new DamageTargetEffect();

			case EffectKind.HealTarget:
				return new HealTargetEffect();

			case EffectKind.None:
			default:
				return null;
		}
	}

	private EffectKind GetEffectKindFromProperty(SerializedProperty p_property)
	{
		object value = p_property.managedReferenceValue;

		if (value == null)
			return EffectKind.None;

		if (value is DamageTargetEffect)
			return EffectKind.DamageTarget;

		if (value is HealTargetEffect)
			return EffectKind.HealTarget;

		return EffectKind.None;
	}
}