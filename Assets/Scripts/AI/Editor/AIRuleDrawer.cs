using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AIRule))]
public class AIRuleDrawer : PropertyDrawer
{
	private enum AIConditionKind
	{
		None,
		EnemyIsAtDistance,
		AllyIsAtDistance,
		HPThreshold,
		HasStatus,
		CanUseAbility,
		ActiveModeIs,
	}

	private enum AIDecisionKind
	{
		None,
		CastAbility,
		MoveUnit,
		EndTurn,
	}

	private const float RowSpacing = 2f;
	private const float SectionSpacing = 4f;
	private const float RemoveButtonWidth = 24f;
	private const float AddButtonWidth = 44f;
	private const float FoldoutWidth = 110f;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		SerializedProperty conditionsProperty = property.FindPropertyRelative("Conditions");
		SerializedProperty decisionProperty = property.FindPropertyRelative("Decision");

		Rect currentRect = new Rect(
			position.x,
			position.y,
			position.width,
			EditorGUIUtility.singleLineHeight
		);

		property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, label, true);

		if (property.isExpanded)
		{
			currentRect.y += currentRect.height + RowSpacing;
			DrawConditionsSection(ref currentRect, conditionsProperty);
			currentRect.y += SectionSpacing;
			DrawDecisionSection(ref currentRect, decisionProperty);
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;

		if (!property.isExpanded)
		{
			return height;
		}

		SerializedProperty conditionsProperty = property.FindPropertyRelative("Conditions");
		SerializedProperty decisionProperty = property.FindPropertyRelative("Decision");

		height += RowSpacing;
		height += GetConditionsSectionHeight(conditionsProperty);
		height += SectionSpacing;
		height += GetDecisionSectionHeight(decisionProperty);

		return height;
	}

	private void DrawConditionsSection(ref Rect currentRect, SerializedProperty conditionsProperty)
	{
		Rect labelRect = new Rect(
			currentRect.x,
			currentRect.y,
			currentRect.width - AddButtonWidth - 4f,
			EditorGUIUtility.singleLineHeight
		);

		Rect addButtonRect = new Rect(
			currentRect.xMax - AddButtonWidth,
			currentRect.y,
			AddButtonWidth,
			EditorGUIUtility.singleLineHeight
		);

		EditorGUI.LabelField(labelRect, "Conditions");

		if (GUI.Button(addButtonRect, "Add"))
		{
			int newIndex = conditionsProperty.arraySize;
			conditionsProperty.InsertArrayElementAtIndex(newIndex);
			SerializedProperty newConditionProperty = conditionsProperty.GetArrayElementAtIndex(newIndex);
			newConditionProperty.managedReferenceValue = null;
			newConditionProperty.isExpanded = true;
			conditionsProperty.serializedObject.ApplyModifiedProperties();
			conditionsProperty.serializedObject.Update();
		}

		currentRect.y += EditorGUIUtility.singleLineHeight + RowSpacing;

		if (conditionsProperty.arraySize == 0)
		{
			EditorGUI.LabelField(currentRect, "No conditions");
			currentRect.y += EditorGUIUtility.singleLineHeight + RowSpacing;
			return;
		}

		for (int i = 0; i < conditionsProperty.arraySize; i++)
		{
			SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);
			DrawConditionEntry(ref currentRect, conditionProperty, i, conditionsProperty);
		}
	}

	private void DrawConditionEntry(
		ref Rect currentRect,
		SerializedProperty conditionProperty,
		int index,
		SerializedProperty conditionsProperty)
	{
		Rect foldoutRect = new Rect(
			currentRect.x,
			currentRect.y,
			FoldoutWidth,
			EditorGUIUtility.singleLineHeight
		);

		Rect popupRect = new Rect(
			foldoutRect.xMax + 4f,
			currentRect.y,
			currentRect.width - FoldoutWidth - RemoveButtonWidth - 8f,
			EditorGUIUtility.singleLineHeight
		);

		Rect removeButtonRect = new Rect(
			currentRect.xMax - RemoveButtonWidth,
			currentRect.y,
			RemoveButtonWidth,
			EditorGUIUtility.singleLineHeight
		);

		conditionProperty.isExpanded = EditorGUI.Foldout(
			foldoutRect,
			conditionProperty.isExpanded,
			$"Condition {index + 1}",
			true
		);

		AIConditionKind currentKind = GetConditionKind(conditionProperty);
		EditorGUI.BeginChangeCheck();
		AIConditionKind newKind = (AIConditionKind)EditorGUI.EnumPopup(popupRect, currentKind);
		if (EditorGUI.EndChangeCheck())
		{
			conditionProperty.managedReferenceValue = CreateConditionInstance(newKind);
			conditionProperty.serializedObject.ApplyModifiedProperties();
			conditionProperty.serializedObject.Update();
		}

		if (GUI.Button(removeButtonRect, "X"))
		{
			conditionsProperty.DeleteArrayElementAtIndex(index);
			conditionsProperty.serializedObject.ApplyModifiedProperties();
			conditionsProperty.serializedObject.Update();
			return;
		}

		currentRect.y += EditorGUIUtility.singleLineHeight + RowSpacing;

		if (conditionProperty.isExpanded && conditionProperty.managedReferenceValue != null)
		{
			DrawManagedReferenceChildren(ref currentRect, conditionProperty);
			currentRect.y += SectionSpacing;
		}
	}

	private void DrawDecisionSection(ref Rect currentRect, SerializedProperty decisionProperty)
	{
		Rect foldoutRect = new Rect(
			currentRect.x,
			currentRect.y,
			FoldoutWidth,
			EditorGUIUtility.singleLineHeight
		);

		Rect popupRect = new Rect(
			foldoutRect.xMax + 4f,
			currentRect.y,
			currentRect.width - FoldoutWidth - 4f,
			EditorGUIUtility.singleLineHeight
		);

		decisionProperty.isExpanded = EditorGUI.Foldout(
			foldoutRect,
			decisionProperty.isExpanded,
			"Decision",
			true
		);

		AIDecisionKind currentKind = GetDecisionKind(decisionProperty);
		EditorGUI.BeginChangeCheck();
		AIDecisionKind newKind = (AIDecisionKind)EditorGUI.EnumPopup(popupRect, currentKind);
		if (EditorGUI.EndChangeCheck())
		{
			decisionProperty.managedReferenceValue = CreateDecisionInstance(newKind);
			decisionProperty.serializedObject.ApplyModifiedProperties();
			decisionProperty.serializedObject.Update();
		}

		currentRect.y += EditorGUIUtility.singleLineHeight + RowSpacing;

		if (decisionProperty.isExpanded && decisionProperty.managedReferenceValue != null)
		{
			DrawManagedReferenceChildren(ref currentRect, decisionProperty);
		}
	}

	private float GetConditionsSectionHeight(SerializedProperty conditionsProperty)
	{
		float height = EditorGUIUtility.singleLineHeight + RowSpacing;

		if (conditionsProperty.arraySize == 0)
		{
			height += EditorGUIUtility.singleLineHeight + RowSpacing;
			return height;
		}

		for (int i = 0; i < conditionsProperty.arraySize; i++)
		{
			SerializedProperty conditionProperty = conditionsProperty.GetArrayElementAtIndex(i);
			height += EditorGUIUtility.singleLineHeight + RowSpacing;

			if (conditionProperty.isExpanded && conditionProperty.managedReferenceValue != null)
			{
				height += GetManagedReferenceChildrenHeight(conditionProperty);
				height += SectionSpacing;
			}
		}

		return height;
	}

	private float GetDecisionSectionHeight(SerializedProperty decisionProperty)
	{
		float height = EditorGUIUtility.singleLineHeight + RowSpacing;

		if (decisionProperty.isExpanded && decisionProperty.managedReferenceValue != null)
		{
			height += GetManagedReferenceChildrenHeight(decisionProperty);
		}

		return height;
	}

	private void DrawManagedReferenceChildren(ref Rect currentRect, SerializedProperty property)
	{
		SerializedProperty iterator = property.Copy();
		SerializedProperty endProperty = iterator.GetEndProperty();

		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
		{
			enterChildren = false;

			if (iterator.depth != property.depth + 1)
			{
				continue;
			}

			float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
			Rect fieldRect = new Rect(
				currentRect.x + 14f,
				currentRect.y,
				currentRect.width - 14f,
				propertyHeight
			);

			EditorGUI.PropertyField(fieldRect, iterator, true);
			currentRect.y += propertyHeight + RowSpacing;
		}
	}

	private float GetManagedReferenceChildrenHeight(SerializedProperty property)
	{
		float height = 0f;

		SerializedProperty iterator = property.Copy();
		SerializedProperty endProperty = iterator.GetEndProperty();

		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
		{
			enterChildren = false;

			if (iterator.depth != property.depth + 1)
			{
				continue;
			}

			height += EditorGUI.GetPropertyHeight(iterator, true);
			height += RowSpacing;
		}

		return height;
	}

	private AICondition CreateConditionInstance(AIConditionKind kind)
	{
		switch (kind)
		{
			case AIConditionKind.EnemyIsAtDistance:
				return new EnemyIsAtDistance();

			case AIConditionKind.AllyIsAtDistance:
				return new AllyIsAtDistance();

			case AIConditionKind.HPThreshold:
				return new HPThreshold();

			case AIConditionKind.HasStatus:
				return new HasStatus();

			case AIConditionKind.CanUseAbility:
				return new CanUseAbility();

			case AIConditionKind.ActiveModeIs:
				return new ActiveModeIs();

			case AIConditionKind.None:
			default:
				return null;
		}
	}

	private AIDecision CreateDecisionInstance(AIDecisionKind kind)
	{
		switch (kind)
		{
			case AIDecisionKind.CastAbility:
				return new CastAbility();

			case AIDecisionKind.MoveUnit:
				return new MoveUnit();

			case AIDecisionKind.EndTurn:
				return new EndTurn();

			case AIDecisionKind.None:
			default:
				return null;
		}
	}

	private AIConditionKind GetConditionKind(SerializedProperty property)
	{
		object value = property.managedReferenceValue;

		if (value == null)
		{
			return AIConditionKind.None;
		}

		if (value is EnemyIsAtDistance)
			return AIConditionKind.EnemyIsAtDistance;

		if (value is AllyIsAtDistance)
			return AIConditionKind.AllyIsAtDistance;

		if (value is HPThreshold)
			return AIConditionKind.HPThreshold;

		if (value is HasStatus)
			return AIConditionKind.HasStatus;

		if (value is CanUseAbility)
			return AIConditionKind.CanUseAbility;

		if (value is ActiveModeIs)
			return AIConditionKind.ActiveModeIs;

		return AIConditionKind.None;
	}

	private AIDecisionKind GetDecisionKind(SerializedProperty property)
	{
		object value = property.managedReferenceValue;

		if (value == null)
		{
			return AIDecisionKind.None;
		}

		if (value is CastAbility)
			return AIDecisionKind.CastAbility;

		if (value is MoveUnit)
			return AIDecisionKind.MoveUnit;

		if (value is EndTurn)
			return AIDecisionKind.EndTurn;

		return AIDecisionKind.None;
	}
}
