#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSceneEntryEmulator))]
public sealed class GameSceneEntryEmulatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		SerializedProperty iterator = serializedObject.GetIterator();
		iterator.NextVisible(true);

		while (iterator.NextVisible(false))
		{
			if (iterator.name == "playerTeam")
			{
				continue;
			}

			EditorGUILayout.PropertyField(iterator, true);
		}

		EditorGUILayout.Space(4f);

		SerializedProperty teamProperty = serializedObject.FindProperty("playerTeam");
		using (new EditorGUI.DisabledScope(teamProperty == null))
		{
			if (GUILayout.Button("Edit Player Team"))
			{
				PlayerTeamEditorWindow.Open(target, "playerTeam");
				GUI.FocusControl(null);
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
