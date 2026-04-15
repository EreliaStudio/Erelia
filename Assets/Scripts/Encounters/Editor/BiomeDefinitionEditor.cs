#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BiomeDefinition))]
public class BiomeDefinitionEditor : Editor
{
	private bool showRawInspector;

	public override void OnInspectorGUI()
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((BiomeDefinition)target), typeof(BiomeDefinition), false);
		}

		EditorGUILayout.Space(6f);
		EditorGUILayout.HelpBox("Use the encounter table window to edit trigger tags, tier weights, and teams.", MessageType.Info);

		if (GUILayout.Button("Open Encounter Table Editor", GUILayout.Height(28f)))
		{
			EncounterTableEditorWindow.Open((BiomeDefinition)target);
		}

		EditorGUILayout.Space(8f);
		showRawInspector = EditorGUILayout.Foldout(showRawInspector, "Raw Inspector");
		if (!showRawInspector)
		{
			return;
		}

		EditorGUILayout.Space(4f);
		DrawDefaultInspector();
	}
}
#endif
