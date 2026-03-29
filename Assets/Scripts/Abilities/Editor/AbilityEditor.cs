using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ability))]
public class AbilityEditor : Editor
{
	private SerializedProperty _costProperty;
	private SerializedProperty _rangeTypeProperty;
	private SerializedProperty _rangeValueProperty;
	private SerializedProperty _requireLineOfSightProperty;
	private SerializedProperty _areaOfEffectTypeProperty;
	private SerializedProperty _areaOfEffectValueProperty;
	private SerializedProperty _targetProfileProperty;
	private SerializedProperty _effectsProperty;

	private const float MainLabelWidth = 120f;

	private enum LineOfSightMode
	{
		NoLineOfSight,
		LineOfSight
	}

	private void OnEnable()
	{
		_costProperty = serializedObject.FindProperty("Cost");
		SerializedProperty rangeProperty = serializedObject.FindProperty("Range");
		_rangeTypeProperty = rangeProperty.FindPropertyRelative("Type");
		_rangeValueProperty = rangeProperty.FindPropertyRelative("Value");
		_requireLineOfSightProperty = rangeProperty.FindPropertyRelative("RequireLineOfSight");
		SerializedProperty areaOfEffectProperty = serializedObject.FindProperty("AreaOfEffect");
		_areaOfEffectTypeProperty = areaOfEffectProperty.FindPropertyRelative("Type");
		_areaOfEffectValueProperty = areaOfEffectProperty.FindPropertyRelative("Value");
		_targetProfileProperty = serializedObject.FindProperty("TargetProfile");
		_effectsProperty = serializedObject.FindProperty("Effects");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		float oldLabelWidth = EditorGUIUtility.labelWidth;
		bool oldWideMode = EditorGUIUtility.wideMode;

		EditorGUIUtility.labelWidth = MainLabelWidth;
		EditorGUIUtility.wideMode = true;

		DrawScriptField();

		EditorGUILayout.PropertyField(_costProperty);

		EditorGUILayout.Space(4);

		DrawRangeLine();
		DrawAreaOfEffectLine();

		EditorGUILayout.PropertyField(_targetProfileProperty);

		EditorGUILayout.Space(6);

		EditorGUILayout.PropertyField(_effectsProperty, true);

		EditorGUIUtility.labelWidth = oldLabelWidth;
		EditorGUIUtility.wideMode = oldWideMode;

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawScriptField()
	{
		using (new EditorGUI.DisabledScope(true))
		{
			MonoScript script = MonoScript.FromScriptableObject((ScriptableObject)target);
			EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
		}
	}

	private void DrawRangeLine()
	{
		Rect rect = EditorGUILayout.GetControlRect();

		float labelWidth = MainLabelWidth;
		float spacing = 4f;

		Rect labelRect = new Rect(
			rect.x,
			rect.y,
			labelWidth - 4f,
			rect.height
		);

		float contentX = rect.x + labelWidth;
		float contentWidth = rect.width - labelWidth;

		float valueWidth = 45f;
		float lineOfSightWidth = 140f;

		float enumWidth = contentWidth - valueWidth - lineOfSightWidth - spacing * 2f;
		if (enumWidth < 60f)
			enumWidth = 60f;

		Rect enumRect = new Rect(
			contentX,
			rect.y,
			enumWidth,
			rect.height
		);

		Rect valueRect = new Rect(
			enumRect.xMax + spacing,
			rect.y,
			valueWidth,
			rect.height
		);

		Rect lineOfSightRect = new Rect(
			valueRect.xMax + spacing,
			rect.y,
			lineOfSightWidth,
			rect.height
		);

		LineOfSightMode currentMode = _requireLineOfSightProperty.boolValue
			? LineOfSightMode.LineOfSight
			: LineOfSightMode.NoLineOfSight;

		EditorGUI.LabelField(labelRect, "Range");

		EditorGUI.PropertyField(enumRect, _rangeTypeProperty, GUIContent.none);
		EditorGUI.PropertyField(valueRect, _rangeValueProperty, GUIContent.none);

		EditorGUI.BeginChangeCheck();
		LineOfSightMode newMode = (LineOfSightMode)EditorGUI.EnumPopup(lineOfSightRect, currentMode);
		if (EditorGUI.EndChangeCheck())
		{
			_requireLineOfSightProperty.boolValue = (newMode == LineOfSightMode.LineOfSight);
		}
	}
	private void DrawAreaOfEffectLine()
	{
		Rect rect = EditorGUILayout.GetControlRect();

		float labelWidth = MainLabelWidth;
		float spacing = 4f;

		Rect labelRect = new Rect(
			rect.x,
			rect.y,
			labelWidth - 4f,
			rect.height
		);

		float contentX = rect.x + labelWidth;
		float contentWidth = rect.width - labelWidth;

		float valueWidth = 45f;
		float enumWidth = contentWidth - valueWidth - spacing;

		if (enumWidth < 60f)
			enumWidth = 60f;

		Rect valueRect = new Rect(
			contentX,
			rect.y,
			valueWidth,
			rect.height
		);

		Rect enumRect = new Rect(
			valueRect.xMax + spacing,
			rect.y,
			enumWidth,
			rect.height
		);

		EditorGUI.LabelField(labelRect, "Area Of Effect");
		EditorGUI.PropertyField(valueRect, _areaOfEffectValueProperty, GUIContent.none);
		EditorGUI.PropertyField(enumRect, _areaOfEffectTypeProperty, GUIContent.none);
	}
}
