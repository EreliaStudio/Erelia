#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public sealed class PlayerTeamEditorWindow : EditorWindow
{
	private const float TopBarHeight = 58f;
	private const float OuterPadding = 10f;
	private const float SectionSpacing = 8f;
	private const float InspectorMinWidth = 320f;
	private const float InspectorWidthRatio = 0.28f;

	[SerializeField] private UnityEngine.Object targetObject;
	[SerializeField] private string teamPropertyPath = string.Empty;
	[SerializeField] private int selectedUnitIndex;

	private readonly EncounterTeamProgressBoardView boardView = new EncounterTeamProgressBoardView();
	private readonly PlayerTeamUnitInspectorView inspectorView = new PlayerTeamUnitInspectorView();

	public static void Open(UnityEngine.Object p_targetObject, string p_teamPropertyPath)
	{
		if (p_targetObject == null || string.IsNullOrEmpty(p_teamPropertyPath))
		{
			return;
		}

		PlayerTeamEditorWindow window = GetWindow<PlayerTeamEditorWindow>("Player Team");
		window.Initialize(p_targetObject, p_teamPropertyPath);
		window.Focus();
	}

	private void OnEnable()
	{
		titleContent = new GUIContent("Player Team");
		minSize = new Vector2(1320f, 760f);
	}

	private void Initialize(UnityEngine.Object p_targetObject, string p_teamPropertyPath)
	{
		targetObject = p_targetObject;
		teamPropertyPath = p_teamPropertyPath ?? string.Empty;
		selectedUnitIndex = Mathf.Clamp(selectedUnitIndex, 0, GameRule.TeamMemberCount - 1);
	}

	private void OnGUI()
	{
		if (!TryGetTeamProperty(out SerializedObject serializedObject, out SerializedProperty teamProperty))
		{
			EditorGUILayout.HelpBox("No player team is selected. Open this window from a PlayerData inspector.", MessageType.Info);
			return;
		}

		EnsureTeamSize(teamProperty);
		serializedObject.Update();
		selectedUnitIndex = Mathf.Clamp(selectedUnitIndex, 0, GameRule.TeamMemberCount - 1);

		CreatureUnit selectedUnit = GetSelectedUnit(teamProperty, selectedUnitIndex, true);
		if (selectedUnit != null)
		{
			FeatProgressionService.ApplyProgress(selectedUnit);
		}

		Rect topRect = new Rect(OuterPadding, OuterPadding, position.width - OuterPadding * 2f, TopBarHeight);
		Rect contentRect = new Rect(
			OuterPadding,
			topRect.yMax + SectionSpacing,
			position.width - OuterPadding * 2f,
			Mathf.Max(0f, position.height - topRect.height - SectionSpacing - OuterPadding * 3f));

		float inspectorWidth = Mathf.Max(InspectorMinWidth, contentRect.width * InspectorWidthRatio);
		Rect boardRect = new Rect(contentRect.x, contentRect.y, contentRect.width - inspectorWidth - SectionSpacing, contentRect.height);
		Rect inspectorRect = new Rect(boardRect.xMax + SectionSpacing, contentRect.y, inspectorWidth, contentRect.height);

		DrawTopUnitBar(topRect, teamProperty);
		DrawBoard(boardRect, selectedUnit, serializedObject);
		DrawInspector(inspectorRect, selectedUnit, serializedObject, teamProperty);
	}

	private void DrawTopUnitBar(Rect rect, SerializedProperty teamProperty)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.88f, 0.88f, 0.88f));

		const float spacing = 6f;
		float buttonWidth = (rect.width - (GameRule.TeamMemberCount - 1) * spacing) / GameRule.TeamMemberCount;

		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			Rect buttonRect = new Rect(rect.x + index * (buttonWidth + spacing), rect.y, buttonWidth, rect.height);
			DrawUnitTab(buttonRect, teamProperty, index);
		}
	}

	private void DrawUnitTab(Rect rect, SerializedProperty teamProperty, int unitIndex)
	{
		CreatureUnit unit = GetSelectedUnit(teamProperty, unitIndex, true);
		CreatureTeamEditorGui.DrawUnitTab(rect, unit, selectedUnitIndex == unitIndex, () =>
		{
			selectedUnitIndex = unitIndex;
			boardView.ClearSelection();
			Repaint();
		});
	}

	private void DrawBoard(Rect rect, CreatureUnit selectedUnit, SerializedObject serializedObject)
	{
		boardView.Draw(rect, selectedUnit, (undoLabel, mutation) => ApplyChange(serializedObject, undoLabel, mutation));
	}

	private void DrawInspector(Rect rect, CreatureUnit selectedUnit, SerializedObject serializedObject, SerializedProperty teamProperty)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.11f, 0.11f, 0.11f) : new Color(0.94f, 0.94f, 0.94f));

		Rect contentRect = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f);
		inspectorView.Draw(
			contentRect,
			teamProperty,
			selectedUnitIndex,
			selectedUnit,
			boardView,
			(undoLabel, mutation) => ApplyChange(serializedObject, undoLabel, mutation));
	}

	private static void ApplyChange(SerializedObject serializedObject, string undoLabel, Action mutation)
	{
		if (serializedObject?.targetObject == null || mutation == null)
		{
			return;
		}

		Undo.RecordObject(serializedObject.targetObject, undoLabel);
		mutation();
		EditorUtility.SetDirty(serializedObject.targetObject);
		serializedObject.Update();
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private bool TryGetTeamProperty(out SerializedObject serializedObject, out SerializedProperty teamProperty)
	{
		serializedObject = null;
		teamProperty = null;

		if (targetObject == null || string.IsNullOrEmpty(teamPropertyPath))
		{
			return false;
		}

		serializedObject = new SerializedObject(targetObject);
		teamProperty = serializedObject.FindProperty(teamPropertyPath);
		return teamProperty != null && teamProperty.isArray;
	}

	private static void EnsureTeamSize(SerializedProperty teamProperty)
	{
		if (teamProperty == null || !teamProperty.isArray)
		{
			return;
		}

		if (teamProperty.arraySize != GameRule.TeamMemberCount)
		{
			teamProperty.arraySize = GameRule.TeamMemberCount;
			teamProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}
	}

	private static CreatureUnit GetSelectedUnit(SerializedProperty teamProperty, int unitIndex, bool createIfMissing)
	{
		if (teamProperty == null || !teamProperty.isArray || unitIndex < 0 || unitIndex >= teamProperty.arraySize)
		{
			return null;
		}

		SerializedProperty elementProperty = teamProperty.GetArrayElementAtIndex(unitIndex);
		if (elementProperty == null)
		{
			return null;
		}

		if (createIfMissing && elementProperty.managedReferenceValue == null)
		{
			elementProperty.managedReferenceValue = new CreatureUnit();
			teamProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

		return elementProperty.managedReferenceValue as CreatureUnit;
	}

}
#endif
