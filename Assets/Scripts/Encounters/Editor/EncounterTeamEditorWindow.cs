#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class EncounterTeamEditorWindow : EditorWindow
{
	private const float ToolbarHeight = 42f;
	private const float TopBarHeight = 74f;
	private const float OuterPadding = 10f;
	private const float SectionSpacing = 8f;
	private const float InspectorMinWidth = 320f;
	private const float InspectorWidthRatio = 0.28f;

	[SerializeField] private BiomeDefinition biome;
	[SerializeField] private string triggerTag = string.Empty;
	[SerializeField] private string tierLabel = string.Empty;
	[SerializeField] private EncounterTier.Entry entry;
	[SerializeField] private int selectedUnitIndex;

	private readonly EncounterTeamProgressBoardView boardView = new EncounterTeamProgressBoardView();
	private readonly EncounterTeamUnitInspectorView inspectorView = new EncounterTeamUnitInspectorView();

	public static void Open(BiomeDefinition ownerBiome, string selectedTriggerTag, string selectedTierLabel, EncounterTier.Entry selectedEntry)
	{
		EncounterTeamEditorWindow window = GetWindow<EncounterTeamEditorWindow>("Encounter Team");
		window.Initialize(ownerBiome, selectedTriggerTag, selectedTierLabel, selectedEntry);
		window.Focus();
	}

	private void OnEnable()
	{
		titleContent = new GUIContent("Encounter Team");
		minSize = new Vector2(1320f, 760f);
	}

	private void Initialize(BiomeDefinition ownerBiome, string selectedTriggerTag, string selectedTierLabel, EncounterTier.Entry selectedEntry)
	{
		biome = ownerBiome;
		triggerTag = selectedTriggerTag ?? string.Empty;
		tierLabel = selectedTierLabel ?? string.Empty;
		entry = selectedEntry;
		selectedUnitIndex = Mathf.Clamp(selectedUnitIndex, 0, GameRule.TeamMemberCount - 1);

		if (entry != null)
		{
			EncounterEditorUtility.EnsureEntry(entry);
		}
	}

	private void OnGUI()
	{
		if (biome == null || entry == null)
		{
			EditorGUILayout.HelpBox("No encounter team is selected. Open this window from the Encounter Table editor.", MessageType.Info);
			return;
		}

		EncounterEditorUtility.EnsureEntry(entry);
		selectedUnitIndex = Mathf.Clamp(selectedUnitIndex, 0, GameRule.TeamMemberCount - 1);

		EncounterUnit selectedUnit = GetSelectedUnit();
		if (selectedUnit != null)
		{
			FeatProgressionService.ApplyProgress(selectedUnit);
		}

		Rect toolbarRect = new Rect(0f, 0f, position.width, ToolbarHeight);
		Rect topRect = new Rect(OuterPadding, toolbarRect.yMax + OuterPadding, position.width - OuterPadding * 2f, TopBarHeight);
		Rect contentRect = new Rect(
			OuterPadding,
			topRect.yMax + SectionSpacing,
			position.width - OuterPadding * 2f,
			Mathf.Max(0f, position.height - toolbarRect.height - topRect.height - SectionSpacing - OuterPadding * 3f));

		float inspectorWidth = Mathf.Max(InspectorMinWidth, contentRect.width * InspectorWidthRatio);
		Rect boardRect = new Rect(contentRect.x, contentRect.y, contentRect.width - inspectorWidth - SectionSpacing, contentRect.height);
		Rect inspectorRect = new Rect(boardRect.xMax + SectionSpacing, contentRect.y, inspectorWidth, contentRect.height);

		DrawToolbar(toolbarRect);
		DrawTopUnitBar(topRect);
		DrawBoard(boardRect, selectedUnit);
		DrawInspector(inspectorRect, selectedUnit);
	}

	private void DrawToolbar(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.86f, 0.86f, 0.86f));

		float x = rect.x + 8f;
		float y = rect.y + 7f;
		float height = rect.height - 14f;

		EditorGUI.LabelField(new Rect(x, y + 1f, 40f, height), "Biome", EditorStyles.miniBoldLabel);
		x += 46f;
		EditorGUI.ObjectField(new Rect(x, y, 220f, height), biome, typeof(BiomeDefinition), false);
		x += 228f;

		EditorGUI.LabelField(new Rect(x, y + 1f, 64f, height), "Trigger", EditorStyles.miniBoldLabel);
		x += 56f;
		EditorGUI.SelectableLabel(new Rect(x, y + 2f, 110f, height), triggerTag);
		x += 118f;

		EditorGUI.LabelField(new Rect(x, y + 1f, 34f, height), "Tier", EditorStyles.miniBoldLabel);
		x += 30f;
		EditorGUI.SelectableLabel(new Rect(x, y + 2f, 120f, height), tierLabel);
		x += 128f;

		EditorGUI.LabelField(new Rect(x, y + 1f, 72f, height), "Team Name", EditorStyles.miniBoldLabel);
		x += 72f;

		string currentName = entry.DisplayName ?? string.Empty;
		string newName = EditorGUI.TextField(new Rect(x, y, Mathf.Max(160f, rect.xMax - x - 12f), height), currentName);
		if (!string.Equals(newName, currentName, StringComparison.Ordinal))
		{
			ApplyChange("Rename Encounter Team", () => entry.DisplayName = newName);
		}
	}

	private void DrawTopUnitBar(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.88f, 0.88f, 0.88f));

		const float spacing = 6f;
		float buttonWidth = (rect.width - (GameRule.TeamMemberCount - 1) * spacing) / GameRule.TeamMemberCount;

		for (int index = 0; index < GameRule.TeamMemberCount; index++)
		{
			Rect buttonRect = new Rect(rect.x + index * (buttonWidth + spacing), rect.y, buttonWidth, rect.height);
			DrawUnitTab(buttonRect, index);
		}
	}

	private void DrawUnitTab(Rect rect, int unitIndex)
	{
		EncounterUnit unit = EncounterEditorUtility.GetOrCreateUnit(entry, unitIndex);
		CreatureTeamEditorGui.DrawUnitTab(rect, unit, selectedUnitIndex == unitIndex, () =>
		{
			selectedUnitIndex = unitIndex;
			boardView.ClearSelection();
			Repaint();
		});
	}

	private void DrawBoard(Rect rect, EncounterUnit selectedUnit)
	{
		boardView.Draw(rect, selectedUnit, ApplyChange);
	}

	private void DrawInspector(Rect rect, EncounterUnit selectedUnit)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.11f, 0.11f, 0.11f) : new Color(0.94f, 0.94f, 0.94f));

		Rect contentRect = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f);
		inspectorView.Draw(contentRect, entry, selectedUnitIndex, selectedUnit, boardView, ApplyChange);
	}

	private EncounterUnit GetSelectedUnit()
	{
		return EncounterEditorUtility.GetOrCreateUnit(entry, selectedUnitIndex);
	}

	private void ApplyChange(string undoLabel, Action mutation)
	{
		if (biome == null || mutation == null)
		{
			return;
		}

		Undo.RecordObject(biome, undoLabel);
		mutation();

		EncounterUnit selectedUnit = GetSelectedUnit();
		if (selectedUnit != null)
		{
			FeatProgressionService.ApplyProgress(selectedUnit);
		}

		EditorUtility.SetDirty(biome);
		Repaint();
	}

}
#endif
