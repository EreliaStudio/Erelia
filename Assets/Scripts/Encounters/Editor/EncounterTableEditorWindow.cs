#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EncounterTableEditorWindow : EditorWindow
{
	private const float ToolbarHeight = 38f;
	private const float SidebarWidth = 220f;
	private const float OuterPadding = 10f;
	private const float SectionSpacing = 8f;
	private const float TierCardMinWidth = 220f;
	private const float TierCardSpacing = 10f;
	private const int TierColumnCount = 5;

	private BiomeDefinition biome;
	private Vector2 triggerListScroll;
	private Vector2 contentScroll;
	private string selectedTriggerTag = string.Empty;
	private string renameBuffer = string.Empty;

	[MenuItem("Tools/Encounter Table Editor")]
	public static void OpenWindow()
	{
		GetWindow<EncounterTableEditorWindow>("Encounter Table");
	}

	public static void Open(BiomeDefinition targetBiome)
	{
		EncounterTableEditorWindow window = GetWindow<EncounterTableEditorWindow>("Encounter Table");
		window.SetBiome(targetBiome);
		window.Focus();
	}

	private void OnEnable()
	{
		titleContent = new GUIContent("Encounter Table");
		minSize = new Vector2(1300f, 760f);
		Selection.selectionChanged += HandleSelectionChanged;

		if (biome == null && Selection.activeObject is BiomeDefinition selectedBiome)
		{
			SetBiome(selectedBiome);
		}
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= HandleSelectionChanged;
	}

	private void OnGUI()
	{
		Rect toolbarRect = new Rect(0f, 0f, position.width, ToolbarHeight);
		Rect bodyRect = new Rect(0f, ToolbarHeight, position.width, Mathf.Max(0f, position.height - ToolbarHeight));
		Rect sidebarRect = new Rect(
			bodyRect.x + OuterPadding,
			bodyRect.y + OuterPadding,
			SidebarWidth,
			Mathf.Max(0f, bodyRect.height - OuterPadding * 2f));
		Rect contentRect = new Rect(
			sidebarRect.xMax + SectionSpacing,
			sidebarRect.y,
			Mathf.Max(0f, bodyRect.width - SidebarWidth - SectionSpacing - OuterPadding * 2f),
			sidebarRect.height);

		DrawToolbar(toolbarRect);
		DrawSidebar(sidebarRect);
		DrawContent(contentRect);
	}

	private void HandleSelectionChanged()
	{
		if (Selection.activeObject is BiomeDefinition selectedBiome && biome == null)
		{
			SetBiome(selectedBiome);
		}
	}

	private void SetBiome(BiomeDefinition targetBiome)
	{
		biome = targetBiome;
		EnsureSelectedTriggerTag();
		Repaint();
	}

	private void DrawToolbar(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.86f, 0.86f, 0.86f));

		float x = rect.x + 8f;
		float y = rect.y + 7f;
		float height = rect.height - 14f;

		Rect biomeLabelRect = new Rect(x, y + 1f, 44f, height);
		GUI.Label(biomeLabelRect, "Biome", EditorStyles.miniBoldLabel);
		x = biomeLabelRect.xMax + 6f;

		Rect biomeFieldRect = new Rect(x, y, 260f, height);
		BiomeDefinition newBiome = (BiomeDefinition)EditorGUI.ObjectField(biomeFieldRect, biome, typeof(BiomeDefinition), false);
		if (newBiome != biome)
		{
			SetBiome(newBiome);
		}

		x = biomeFieldRect.xMax + 6f;
		if (GUI.Button(new Rect(x, y, 94f, height), "Use Selection", EditorStyles.toolbarButton))
		{
			if (Selection.activeObject is BiomeDefinition selectedBiome)
			{
				SetBiome(selectedBiome);
			}
		}

		x += 100f;
		using (new EditorGUI.DisabledScope(biome == null))
		{
			if (GUI.Button(new Rect(x, y, 110f, height), "Add Trigger Tag", EditorStyles.toolbarButton))
			{
				AddTriggerTag();
			}
		}
	}

	private void DrawSidebar(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.94f, 0.94f, 0.94f));

		Rect contentRect = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f);
		GUILayout.BeginArea(contentRect);

		EditorGUILayout.LabelField("Trigger Tags", EditorStyles.boldLabel);
		EditorGUILayout.Space(4f);

		if (biome == null)
		{
			EditorGUILayout.HelpBox("Pick a biome definition to edit its encounter rules.", MessageType.Info);
			GUILayout.EndArea();
			return;
		}

		List<string> triggerTags = GetTriggerTags();
		triggerListScroll = EditorGUILayout.BeginScrollView(triggerListScroll);
		for (int index = 0; index < triggerTags.Count; index++)
		{
			string triggerTag = triggerTags[index];
			bool isSelected = string.Equals(triggerTag, selectedTriggerTag, StringComparison.Ordinal);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Toggle(isSelected, triggerTag, "Button"))
			{
				if (!isSelected)
				{
					selectedTriggerTag = triggerTag;
					renameBuffer = triggerTag;
					GUI.FocusControl(null);
				}
			}

			using (new EditorGUI.DisabledScope(triggerTags.Count <= 1))
			{
				if (GUILayout.Button("X", GUILayout.Width(22f)))
				{
					RemoveTriggerTag(triggerTag);
					GUIUtility.ExitGUI();
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.Space(4f);

		if (GUILayout.Button("Add Trigger Tag"))
		{
			AddTriggerTag();
		}

		GUILayout.EndArea();
	}

	private void DrawContent(Rect rect)
	{
		EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.10f, 0.10f, 0.10f) : new Color(0.97f, 0.97f, 0.97f));

		Rect contentRect = new Rect(rect.x + 10f, rect.y + 10f, rect.width - 20f, rect.height - 20f);
		GUILayout.BeginArea(contentRect);

		if (biome == null)
		{
			EditorGUILayout.HelpBox("Select a biome definition to edit encounter tiers.", MessageType.Info);
			GUILayout.EndArea();
			return;
		}

		EnsureSelectedTriggerTag();
		if (!TryGetSelectedRule(out BiomeEncounterRule rule))
		{
			EditorGUILayout.HelpBox("This biome has no trigger tag rules yet. Add one from the sidebar.", MessageType.Info);
			GUILayout.EndArea();
			return;
		}

		EncounterEditorUtility.EnsureRule(rule);
		DrawRuleHeader(rule);
		EditorGUILayout.Space(8f);

		contentScroll = EditorGUILayout.BeginScrollView(contentScroll);
		DrawTierGrid(rule);
		EditorGUILayout.EndScrollView();

		GUILayout.EndArea();
	}

	private void DrawRuleHeader(BiomeEncounterRule rule)
	{
		EditorGUILayout.LabelField("Selected Rule", EditorStyles.boldLabel);
		EditorGUILayout.Space(2f);

		string newRenameBuffer = EditorGUILayout.TextField("Trigger Tag", renameBuffer);
		if (!string.Equals(newRenameBuffer, renameBuffer, StringComparison.Ordinal))
		{
			renameBuffer = newRenameBuffer;
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(renameBuffer) ||
		                                   BiomeDefinition.AreTriggerTagsEquivalent(renameBuffer, selectedTriggerTag)))
		{
			if (GUILayout.Button("Apply Tag Rename", GUILayout.Width(140f)))
			{
				RenameSelectedTriggerTag(renameBuffer);
				GUI.FocusControl(null);
			}
		}

		EditorGUILayout.EndHorizontal();

		float newChance = EditorGUILayout.Slider("Base Chance Per Step", rule.BaseChancePerStep, 0f, 1f);
		if (!Mathf.Approximately(newChance, rule.BaseChancePerStep))
		{
			ApplyBiomeChange("Edit Encounter Base Chance", () => rule.BaseChancePerStep = newChance);
		}
	}

	private void DrawTierGrid(BiomeEncounterRule rule)
	{
		float availableWidth = Mathf.Max(0f, position.width - SidebarWidth - OuterPadding * 4f - SectionSpacing);
		float cardWidth = Mathf.Max(TierCardMinWidth, (availableWidth - TierCardSpacing * (TierColumnCount - 1)) / TierColumnCount);

		for (int row = 0; row < 2; row++)
		{
			EditorGUILayout.BeginHorizontal();

			for (int column = 0; column < TierColumnCount; column++)
			{
				int tierIndex = row * TierColumnCount + column;
				DrawTierCard(EncounterEditorUtility.GetTier(rule.EncounterTable, tierIndex), tierIndex, cardWidth);
				if (column < TierColumnCount - 1)
				{
					GUILayout.Space(TierCardSpacing);
				}
			}

			EditorGUILayout.EndHorizontal();
			if (row == 0)
			{
				EditorGUILayout.Space(TierCardSpacing);
			}
		}
	}

	private void DrawTierCard(EncounterTier tier, int tierIndex, float width)
	{
		EncounterEditorUtility.EnsureTier(tier);

		EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width), GUILayout.MinHeight(220f));
		EditorGUILayout.LabelField(EncounterEditorUtility.TierLabels[tierIndex], EditorStyles.boldLabel);
		EditorGUILayout.Space(4f);

		if (tier == null || tier.WeightedTeams == null || tier.WeightedTeams.Count == 0)
		{
			EditorGUILayout.HelpBox("No teams in this tier.", MessageType.None);
		}
		else
		{
			for (int entryIndex = 0; entryIndex < tier.WeightedTeams.Count; entryIndex++)
			{
				EncounterTier.Entry entry = tier.WeightedTeams[entryIndex];
				EncounterEditorUtility.EnsureEntry(entry);
				DrawTierEntryRow(tier, tierIndex, entry, entryIndex);
			}
		}

		GUILayout.FlexibleSpace();
		if (GUILayout.Button("+ Team"))
		{
			AddTierEntry(tier);
		}

		EditorGUILayout.EndVertical();
	}

	private void DrawTierEntryRow(EncounterTier tier, int tierIndex, EncounterTier.Entry entry, int entryIndex)
	{
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("X", GUILayout.Width(22f)))
		{
			RemoveTierEntry(tier, entryIndex);
			GUIUtility.ExitGUI();
		}

		string currentName = entry.DisplayName ?? string.Empty;
		string newName = EditorGUILayout.TextField(currentName, GUILayout.MinWidth(90f));
		if (!string.Equals(newName, currentName, StringComparison.Ordinal))
		{
			ApplyBiomeChange("Rename Encounter Team", () => entry.DisplayName = newName);
		}

		int newWeight = EditorGUILayout.IntField(entry.Weight, GUILayout.Width(40f));
		if (newWeight != entry.Weight)
		{
			ApplyBiomeChange("Edit Encounter Weight", () => entry.Weight = Mathf.Max(0, newWeight));
		}

		if (GUILayout.Button("Edit", GUILayout.Width(42f)))
		{
			EncounterTeamEditorWindow.Open(biome, selectedTriggerTag, EncounterEditorUtility.TierLabels[tierIndex], entry);
		}

		EditorGUILayout.EndHorizontal();
	}

	private void AddTriggerTag()
	{
		if (biome == null)
		{
			return;
		}

		string newTag = BuildUniqueTriggerTag("area");
		ApplyBiomeChange("Add Encounter Trigger Tag", () =>
		{
			biome.WildEncounterRulesByTriggerTag.Add(newTag, new BiomeEncounterRule
			{
				BaseChancePerStep = 0.1f,
				EncounterTable = new EncounterTable()
			});
		});

		selectedTriggerTag = newTag;
		renameBuffer = newTag;
	}

	private void RemoveTriggerTag(string triggerTag)
	{
		if (biome == null || string.IsNullOrEmpty(triggerTag) || !biome.WildEncounterRulesByTriggerTag.ContainsKey(triggerTag))
		{
			return;
		}

		ApplyBiomeChange("Remove Encounter Trigger Tag", () => biome.WildEncounterRulesByTriggerTag.Remove(triggerTag));
		EnsureSelectedTriggerTag();
	}

	private void RenameSelectedTriggerTag(string proposedTag)
	{
		if (biome == null || string.IsNullOrEmpty(selectedTriggerTag))
		{
			return;
		}

		string cleanedTag = BiomeDefinition.CleanTriggerTag(proposedTag);
		if (string.IsNullOrEmpty(cleanedTag) || BiomeDefinition.AreTriggerTagsEquivalent(cleanedTag, selectedTriggerTag))
		{
			return;
		}

		if (ContainsEquivalentTriggerTag(cleanedTag))
		{
			EditorUtility.DisplayDialog("Trigger Tag Already Exists", $"The trigger tag '{cleanedTag}' already exists on this biome.", "OK");
			return;
		}

		if (!biome.WildEncounterRulesByTriggerTag.TryGetValue(selectedTriggerTag, out BiomeEncounterRule existingRule))
		{
			return;
		}

		string previousTag = selectedTriggerTag;
		ApplyBiomeChange("Rename Encounter Trigger Tag", () =>
		{
			biome.WildEncounterRulesByTriggerTag.Remove(previousTag);
			biome.WildEncounterRulesByTriggerTag.Add(cleanedTag, existingRule);
		});

		selectedTriggerTag = cleanedTag;
		renameBuffer = cleanedTag;
	}

	private void AddTierEntry(EncounterTier tier)
	{
		if (biome == null || tier == null)
		{
			return;
		}

		ApplyBiomeChange("Add Encounter Team", () =>
		{
			tier.WeightedTeams.Add(EncounterEditorUtility.CreateEntry(tier.WeightedTeams.Count));
		});
	}

	private void RemoveTierEntry(EncounterTier tier, int entryIndex)
	{
		if (biome == null || tier?.WeightedTeams == null || entryIndex < 0 || entryIndex >= tier.WeightedTeams.Count)
		{
			return;
		}

		ApplyBiomeChange("Remove Encounter Team", () => tier.WeightedTeams.RemoveAt(entryIndex));
	}

	private bool TryGetSelectedRule(out BiomeEncounterRule rule)
	{
		rule = null;
		return biome != null &&
		       !string.IsNullOrEmpty(selectedTriggerTag) &&
		       biome.WildEncounterRulesByTriggerTag != null &&
		       biome.WildEncounterRulesByTriggerTag.TryGetValue(selectedTriggerTag, out rule) &&
		       rule != null;
	}

	private void EnsureSelectedTriggerTag()
	{
		if (biome == null || biome.WildEncounterRulesByTriggerTag == null || biome.WildEncounterRulesByTriggerTag.Count == 0)
		{
			selectedTriggerTag = string.Empty;
			renameBuffer = string.Empty;
			return;
		}

		if (!string.IsNullOrEmpty(selectedTriggerTag) && biome.WildEncounterRulesByTriggerTag.ContainsKey(selectedTriggerTag))
		{
			if (string.IsNullOrEmpty(renameBuffer))
			{
				renameBuffer = selectedTriggerTag;
			}

			return;
		}

		foreach (var entry in biome.WildEncounterRulesByTriggerTag)
		{
			selectedTriggerTag = entry.Key;
			renameBuffer = selectedTriggerTag;
			return;
		}
	}

	private List<string> GetTriggerTags()
	{
		var tags = new List<string>();
		if (biome?.WildEncounterRulesByTriggerTag == null)
		{
			return tags;
		}

		foreach (var entry in biome.WildEncounterRulesByTriggerTag)
		{
			tags.Add(entry.Key);
		}

		return tags;
	}

	private string BuildUniqueTriggerTag(string baseTag)
	{
		string cleanedBaseTag = BiomeDefinition.CleanTriggerTag(baseTag);
		string rootTag = string.IsNullOrEmpty(cleanedBaseTag) ? "trigger" : cleanedBaseTag;
		string candidate = rootTag;
		int suffix = 1;

		while (ContainsEquivalentTriggerTag(candidate))
		{
			candidate = $"{rootTag}_{suffix}";
			suffix++;
		}

		return candidate;
	}

	private bool ContainsEquivalentTriggerTag(string candidate)
	{
		if (biome?.WildEncounterRulesByTriggerTag == null || string.IsNullOrWhiteSpace(candidate))
		{
			return false;
		}

		foreach (var entry in biome.WildEncounterRulesByTriggerTag)
		{
			if (BiomeDefinition.AreTriggerTagsEquivalent(entry.Key, candidate))
			{
				return true;
			}
		}

		return false;
	}

	private void ApplyBiomeChange(string undoLabel, Action mutation)
	{
		if (biome == null || mutation == null)
		{
			return;
		}

		Undo.RecordObject(biome, undoLabel);
		mutation();
		EditorUtility.SetDirty(biome);
		Repaint();
	}
}
#endif
