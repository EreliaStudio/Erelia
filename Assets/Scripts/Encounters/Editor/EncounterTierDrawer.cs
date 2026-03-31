using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EncounterTier))]
public class EncounterTierDrawer : PropertyDrawer
{
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("WeightedTeams");

        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

        if (property.isExpanded)
        {
            float y = foldoutRect.yMax + Spacing;

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);

                Rect lineRect = new Rect(position.x + 10, y, position.width - 10, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(lineRect, element, GUIContent.none);

                // Remove button
                Rect removeRect = new Rect(position.x + position.width - 20, y, 20, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(removeRect, "-"))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                y += EditorGUIUtility.singleLineHeight + Spacing;
            }

            // Add button
            Rect addRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "+ Add Team"))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);

                SerializedProperty newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                SerializedProperty team = newElement.FindPropertyRelative("Team");

                // Ensure correct size
                team.arraySize = GameRule.TeamMemberCount;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        SerializedProperty listProp = property.FindPropertyRelative("WeightedTeams");

        float height = EditorGUIUtility.singleLineHeight; // foldout

        height += (EditorGUIUtility.singleLineHeight + Spacing) * listProp.arraySize;

        height += EditorGUIUtility.singleLineHeight; // add button

        return height;
    }
}