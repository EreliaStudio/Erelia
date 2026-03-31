using UnityEditor;
using UnityEngine;

public class EncounterTeamEditorWindow : EditorWindow
{
	private const int ColumnCount = 3;
	private const float CardSpacing = 8f;

	private SerializedObject _serializedObject;
	private SerializedProperty _teamProperty;
	private Vector2 _scroll;

	private EncounterTeamUnitEditor[] _unitEditors;

	public static void Open(SerializedObject p_serializedObject, SerializedProperty p_teamProperty)
	{
		if (p_serializedObject == null || p_teamProperty == null)
		{
			Debug.LogError("Cannot open EncounterTeamEditorWindow with null data.");
			return;
		}

		EncounterTeamEditorWindow window = GetWindow<EncounterTeamEditorWindow>("Encounter Team");
		window.Initialize(p_serializedObject, p_teamProperty);
	}

	private void Initialize(SerializedObject p_serializedObject, SerializedProperty p_teamProperty)
	{
		_serializedObject = p_serializedObject;
		_teamProperty = p_teamProperty;

		EnsureArraySize();
		BuildUnitEditors();
	}

	private void OnGUI()
	{
		if (_serializedObject == null || _teamProperty == null)
		{
			EditorGUILayout.HelpBox("No team selected.", MessageType.Info);
			return;
		}

		_serializedObject.Update();

		EnsureArraySize();

		if (_unitEditors == null || _unitEditors.Length != _teamProperty.arraySize)
		{
			BuildUnitEditors();
		}

		_scroll = EditorGUILayout.BeginScrollView(_scroll);

		DrawTeamGrid();

		EditorGUILayout.EndScrollView();

		_serializedObject.ApplyModifiedProperties();
	}

	private void EnsureArraySize()
	{
		if (_teamProperty.arraySize != GameRule.TeamMemberCount)
		{
			_teamProperty.arraySize = GameRule.TeamMemberCount;
		}
	}

	private void BuildUnitEditors()
	{
		_unitEditors = new EncounterTeamUnitEditor[_teamProperty.arraySize];

		for (int index = 0; index < _teamProperty.arraySize; index++)
		{
			SerializedProperty unitProperty = _teamProperty.GetArrayElementAtIndex(index);
			_unitEditors[index] = new EncounterTeamUnitEditor(index, unitProperty);
		}
	}

	private void DrawTeamGrid()
	{
		float totalWidth = position.width - 24f;
		float cardWidth = (totalWidth - (ColumnCount - 1) * CardSpacing) / ColumnCount;

		for (int index = 0; index < _unitEditors.Length; index += ColumnCount)
		{
			EditorGUILayout.BeginHorizontal();

			for (int column = 0; column < ColumnCount; column++)
			{
				int unitIndex = index + column;

				if (unitIndex >= _unitEditors.Length)
				{
					GUILayout.FlexibleSpace();
					continue;
				}

				_unitEditors[unitIndex].Draw(cardWidth);
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(CardSpacing);
		}
	}
}