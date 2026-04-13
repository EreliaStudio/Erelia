using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class FeatBoardEditorWindow
{
	private void ApplySpeciesChange(string undoLabel, Action mutation)
	{
		if (species == null || mutation == null)
		{
			return;
		}

		Undo.RecordObject(species, undoLabel);
		mutation();
		MarkSpeciesDirty();
	}

	private void MarkSpeciesDirty()
	{
		if (species == null)
		{
			return;
		}

		EditorUtility.SetDirty(species);
		Repaint();
	}

	private List<FeatNode> GetNodes()
	{
		return species?.FeatBoard?.Nodes ?? EmptyNodeList;
	}

	private FeatNode GetSelectedNode()
	{
		return selectedNode;
	}

	private FeatNode GetRootNode()
	{
		if (species == null || species.FeatBoard == null)
		{
			return null;
		}

		return species.FeatBoard.GetRootNode();
	}

	private bool IsRootNode(FeatNode p_node)
	{
		return species?.FeatBoard != null && species.FeatBoard.IsRootNode(p_node);
	}

	private void SetRootNode(FeatNode p_node)
	{
		if (species == null)
		{
			return;
		}

		ApplySpeciesChange("Set Feat Root Node", () =>
		{
			species.FeatBoard.RootNodeId = p_node != null ? p_node.Id : string.Empty;
		});
	}

	private FeatNode GetNodeAtCanvasPosition(Vector2 localPosition)
	{
		List<FeatNode> drawOrder = GetDrawOrder();

		for (int i = drawOrder.Count - 1; i >= 0; i--)
		{
			FeatNode node = drawOrder[i];
			if (node != null && GetNodeRect(node).Contains(localPosition))
			{
				return node;
			}
		}

		return null;
	}

	private List<FeatNode> GetDrawOrder()
	{
		List<FeatNode> nodes = GetNodes();
		List<FeatNode> drawOrder = new List<FeatNode>(nodes.Count);
		FeatNode currentSelectedNode = GetSelectedNode();

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node != null && node != currentSelectedNode)
			{
				drawOrder.Add(node);
			}
		}

		if (currentSelectedNode != null)
		{
			drawOrder.Add(currentSelectedNode);
		}

		return drawOrder;
	}

	private Rect GetNodeRect(FeatNode node)
	{
		Vector2 size = NodeGridSize * GetScaledGridSize();
		Vector2 position = BoardToCanvas(node.Position);
		return new Rect(position, size);
	}

	private Vector2 BoardToCanvas(Vector2 boardPosition)
	{
		return canvasPan + boardPosition * GetScaledGridSize();
	}

	private Vector2 CanvasToBoard(Vector2 canvasPosition)
	{
		return (canvasPosition - canvasPan) / GetScaledGridSize();
	}

	private float GetScaledGridSize()
	{
		return BaseGridSize * zoom;
	}

	private Vector2 SnapToGrid(Vector2 positionToSnap)
	{
		return new Vector2(Mathf.Round(positionToSnap.x), Mathf.Round(positionToSnap.y));
	}

	private string BuildNodeBody(FeatNode node)
	{
		List<string> lines = new List<string>
		{
			FormatGridLine(node),
			string.Empty,
			"Requirement"
		};

		AppendSummaryLines(lines, node.Requirements, FormatRequirementSummary);
		lines.Add(string.Empty);
		lines.Add("Reward");
		AppendSummaryLines(lines, node.Rewards, FormatRewardSummary);

		return string.Join("\n", lines);
	}

	private void AppendSummaryLines<T>(List<string> lines, List<T> entries, Func<T, string> formatter) where T : class
	{
		bool hasEntries = false;

		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				T entry = entries[i];
				if (entry == null)
				{
					continue;
				}

				lines.Add(formatter(entry));
				hasEntries = true;
			}
		}

		if (!hasEntries)
		{
			lines.Add("None");
		}
	}

	private RepeatTimeMode GetRepeatTimeMode(int numberOfRepeatTime)
	{
		if (numberOfRepeatTime < 0)
		{
			return RepeatTimeMode.Infinite;
		}

		return numberOfRepeatTime > 0 ? RepeatTimeMode.Repeatable : RepeatTimeMode.Unique;
	}

	private string FormatGridLine(FeatNode node)
	{
		string gridText = "Grid " + FormatVector2(node.Position);
		if (node == null || node.Kind != FeatNodeKind.StatsBonus)
		{
			return gridText;
		}

		switch (GetRepeatTimeMode(node.NumberOfRepeatTime))
		{
			case RepeatTimeMode.Repeatable:
				return gridText + " | Repeat " + node.NumberOfRepeatTime + " times";

			case RepeatTimeMode.Infinite:
				return gridText + " | Infinite";

			default:
				return gridText + " | Unique";
		}
	}

	private string FormatRewardSummary(FeatReward reward)
	{
		switch (reward)
		{
			case BonusStatsReward bonusStats:
				return (bonusStats.Value >= 0 ? "+" : string.Empty) + bonusStats.Value + " " + ObjectNames.NicifyVariableName(bonusStats.Attribute.ToString());

			case AbilityReward abilityReward:
				return "Ability: " + (abilityReward.Ability != null ? abilityReward.Ability.name : "Unassigned");

			case PassiveReward passiveReward:
				return "Passive: " + (passiveReward.Status != null ? passiveReward.Status.name : "Unassigned");

			case ChangeFormReward changeFormReward:
				return "Form: " + (string.IsNullOrWhiteSpace(changeFormReward.FormKey) ? "Unassigned" : changeFormReward.FormKey);

			default:
				return "Unknown reward";
		}
	}

	private string FormatRequirementSummary(FeatRequirement requirement)
	{
		switch (requirement)
		{
			case DealDamageRequirement dealDamage:
				return "Deal " + dealDamage.RequiredAmount + " damage";

			case HealHealthRequirement healHealth:
				return "Heal " + healHealth.RequiredAmount;

			default:
				return "Unknown requirement";
		}
	}

	private string GetNodeLabel(FeatNode node)
	{
		if (node == null)
		{
			return "Missing Node";
		}

		string name = string.IsNullOrWhiteSpace(node.DisplayName) ? "Unnamed Node" : node.DisplayName;
		return name + " [" + GetKindLabel(node.Kind) + "]";
	}

	private string GetKindLabel(FeatNodeKind kind)
	{
		return ObjectNames.NicifyVariableName(kind.ToString());
	}

	private string FormatVector2(Vector2 value)
	{
		return Mathf.RoundToInt(value.x) + ", " + Mathf.RoundToInt(value.y);
	}

	private List<string> GetSpeciesFormKeys()
	{
		List<string> keys = new List<string>();
		if (species == null || species.Forms == null)
		{
			return keys;
		}

		foreach (var entry in species.Forms)
		{
			if (!string.IsNullOrWhiteSpace(entry.Key))
			{
				keys.Add(entry.Key);
			}
		}

		keys.Sort(StringComparer.OrdinalIgnoreCase);
		return keys;
	}

	private Dictionary<FeatNode, int> BuildNodeIndexLookup(List<FeatNode> nodes)
	{
		Dictionary<FeatNode, int> lookup = new Dictionary<FeatNode, int>();

		for (int i = 0; i < nodes.Count; i++)
		{
			FeatNode node = nodes[i];
			if (node != null && !lookup.ContainsKey(node))
			{
				lookup.Add(node, i);
			}
		}

		return lookup;
	}
}
