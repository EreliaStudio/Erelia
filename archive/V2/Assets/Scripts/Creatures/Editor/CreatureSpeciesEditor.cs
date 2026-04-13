using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreatureSpecies))]
public class CreatureSpeciesEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space(8);

		if (GUILayout.Button("Open Feat Board"))
		{
			var species = (CreatureSpecies)target;
			FeatBoardEditorWindow.Open(species);
		}
	}
}