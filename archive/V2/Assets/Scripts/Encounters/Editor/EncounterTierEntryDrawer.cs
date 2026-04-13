using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EncounterTier.Entry))]
public class EncounterTierEntryDrawer : PropertyDrawer
{
    private const float ButtonWidth = 100f;
    private const float Spacing = 5f;

    public override void OnGUI(Rect p_position, SerializedProperty p_property, GUIContent p_label)
    {
        SerializedProperty weightProp = p_property.FindPropertyRelative("Weight");
        SerializedProperty teamProp = p_property.FindPropertyRelative("Team");

        Rect weightRect = new Rect(
            p_position.x,
            p_position.y,
            p_position.width - ButtonWidth - Spacing,
            EditorGUIUtility.singleLineHeight
        );

        Rect buttonRect = new Rect(
            weightRect.xMax + Spacing,
            p_position.y,
            ButtonWidth,
            EditorGUIUtility.singleLineHeight
        );

        EditorGUI.PropertyField(weightRect, weightProp, GUIContent.none);

        if (GUI.Button(buttonRect, "Edit Team"))
        {
            OpenTeamEditor(p_property, teamProp);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    private void OpenTeamEditor(SerializedProperty p_entryProperty, SerializedProperty p_teamProperty)
    {
        EncounterTeamEditorWindow.Open(
            p_entryProperty.serializedObject,
            p_teamProperty
        );
    }
}