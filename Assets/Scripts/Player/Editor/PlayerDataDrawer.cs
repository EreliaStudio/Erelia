#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PlayerData))]
public sealed class PlayerDataDrawer : PropertyDrawer
{
	private const float VerticalSpacing = 4f;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (property == null)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		float height = EditorGUIUtility.singleLineHeight;
		if (!property.isExpanded)
		{
			return height;
		}

		height += VerticalSpacing + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("movementSpeed"), true);
		height += VerticalSpacing + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("worldCell"), true);
		height += VerticalSpacing + EditorGUIUtility.singleLineHeight;
		return height;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;
			float y = foldoutRect.yMax + VerticalSpacing;

			SerializedProperty movementSpeedProperty = property.FindPropertyRelative("movementSpeed");
			SerializedProperty worldCellProperty = property.FindPropertyRelative("worldCell");
			SerializedProperty teamProperty = property.FindPropertyRelative("team");

			if (movementSpeedProperty != null)
			{
				float height = EditorGUI.GetPropertyHeight(movementSpeedProperty, true);
				Rect fieldRect = new Rect(position.x, y, position.width, height);
				EditorGUI.PropertyField(fieldRect, movementSpeedProperty, true);
				y = fieldRect.yMax + VerticalSpacing;
			}

			if (worldCellProperty != null)
			{
				float height = EditorGUI.GetPropertyHeight(worldCellProperty, true);
				Rect fieldRect = new Rect(position.x, y, position.width, height);
				EditorGUI.PropertyField(fieldRect, worldCellProperty, true);
				y = fieldRect.yMax + VerticalSpacing;
			}

			Rect buttonRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
			using (new EditorGUI.DisabledScope(teamProperty == null || property.serializedObject?.targetObject == null))
			{
				if (GUI.Button(buttonRect, "Edit Team"))
				{
					PlayerTeamEditorWindow.Open(property.serializedObject.targetObject, teamProperty.propertyPath);
					GUI.FocusControl(null);
				}
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}
}
#endif
