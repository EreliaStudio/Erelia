#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(BattleBoard))]
public class BattleBoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty dataProp = serializedObject.FindProperty("data");
        SerializedProperty viewProp = serializedObject.FindProperty("view");

        if (dataProp != null)
        {
            EditorGUILayout.PropertyField(dataProp, true);
        }

        if (viewProp != null)
        {
            EditorGUILayout.PropertyField(viewProp, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
