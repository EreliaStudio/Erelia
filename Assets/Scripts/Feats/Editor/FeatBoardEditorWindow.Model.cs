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

			case RemoveAbilityReward removeAbilityReward:
				return "Remove Ability: " + (removeAbilityReward.Ability != null ? removeAbilityReward.Ability.name : "Unassigned");

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
		if (requirement == null)
		{
			return "Missing requirement";
		}

		string duration = FormatRequirementDuration(requirement);

		switch (requirement)
		{
			// ── Damage ──────────────────────────────────────────────────────────
			case DealDamageRequirement dealDamage:
				return "Deal " + dealDamage.RequiredAmount + FormatDamageKindFilter(dealDamage.DamageKind) + " damage " + duration;

			case TakeDamageRequirement takeDamage:
				return "Take " + takeDamage.RequiredAmount + FormatDamageKindFilter(takeDamage.DamageKind) + " damage " + duration;

			case WinAfterDealingDamageRequirement winDamage:
				return "Deal " + winDamage.RequiredAmount + " damage and win " + duration;

			case SurviveHitRequirement surviveHit:
				return "Survive a hit of " + surviveHit.RequiredAmount + "+ damage " + duration;

			// ── Healing ─────────────────────────────────────────────────────────
			case HealHealthRequirement healHealth:
				return "Heal " + healHealth.RequiredAmount + " HP " + duration;

			case HealTargetRequirement healTarget:
				string healTargetLabel = healTarget.Target switch
				{
					HealTargetRequirement.TargetFilter.Self => "yourself",
					HealTargetRequirement.TargetFilter.Ally => "an ally",
					_ => "a target"
				};
				return "Heal " + healTarget.RequiredAmount + " HP to " + healTargetLabel + " " + duration;

			case WinAfterHealingRequirement winHeal:
				return "Heal " + winHeal.RequiredAmount + " HP and win " + duration;

			// ── Shields ─────────────────────────────────────────────────────────
			case ApplyShieldRequirement applyShield:
				string shieldKindLabel = applyShield.Filter switch
				{
					ApplyShieldRequirement.KindFilter.Physical => " physical",
					ApplyShieldRequirement.KindFilter.Magical => " magical",
					_ => string.Empty
				};
				return "Apply " + applyShield.RequiredAmount + shieldKindLabel + " shield " + duration;

			case ApplyShieldCountRequirement applyShieldCount:
				return "Apply a shield " + applyShieldCount.RequiredCount + " times " + duration;

			case AbsorbDamageWithShieldRequirement absorbDamage:
				return "Absorb " + absorbDamage.RequiredAmount + " damage with a shield " + duration;

			case MaxDamageAbsorbedInOneHitRequirement maxAbsorb:
				return "Absorb " + maxAbsorb.RequiredAmount + "+ damage with a shield in one hit";

			case ShieldBrokenRequirement shieldBroken:
				string brokenKindLabel = shieldBroken.Filter switch
				{
					ShieldBrokenRequirement.KindFilter.Physical => " physical",
					ShieldBrokenRequirement.KindFilter.Magical => " magical",
					_ => string.Empty
				};
				return "Have " + shieldBroken.RequiredCount + brokenKindLabel + " shield(s) broken " + duration;

			// ── Status ──────────────────────────────────────────────────────────
			case ApplyStatusCountRequirement applyStatus:
				string applyStatusLabel = applyStatus.RequiredStatus != null ? applyStatus.RequiredStatus.name : "a status";
				return "Apply " + applyStatusLabel + " " + applyStatus.RequiredCount + " times " + duration;

			case RemoveStatusCountRequirement removeStatus:
				string removeStatusLabel = removeStatus.RequiredStatus != null ? removeStatus.RequiredStatus.name : "a status";
				return "Remove " + removeStatusLabel + " " + removeStatus.RequiredCount + " times " + duration;

			// ── Kills ────────────────────────────────────────────────────────────
			case KillCountRequirement killCount:
				return "Defeat " + killCount.RequiredCount + " enemies " + duration;

			case LastHitRequirement lastHit:
				return "Deliver the finishing blow " + lastHit.RequiredCount + " times " + duration;

			// ── Battle outcome ───────────────────────────────────────────────────
			case WinBattleCountRequirement winBattle:
				return "Win a battle" + (winBattle.RequireUnitSurvival ? " without falling" : string.Empty) + " " + duration;

			// ── Resources ────────────────────────────────────────────────────────
			case ConsumeResourcesRequirement consumeRes:
				string resourceLabel = consumeRes.RequiredResource switch
				{
					ResourceConsumedEvent.ResourceKind.ActionPoints => "action points",
					_ => consumeRes.RequiredResource.ToString().ToLowerInvariant()
				};
				return "Spend " + consumeRes.RequiredAmount + " " + resourceLabel + " " + duration;

			// ── Movement ─────────────────────────────────────────────────────────
			case TotalDistanceTravelledRequirement distanceTravelled:
				return "Move " + distanceTravelled.RequiredDistance + " tiles " + duration;

			case DisplacementDealtRequirement displacementDealt:
				string displaceDealtLabel = !displacementDealt.FilterByOrientation ? "displace" :
					displacementDealt.RequiredOrientation == MoveStatus.Orientation.AwayFromCaster ? "push" : "pull";
				return displacementDealt.RequiredDistance + " tiles of " + displaceDealtLabel + " dealt " + duration;

			case DisplacementReceivedRequirement displacementReceived:
				string displaceReceivedLabel = !displacementReceived.FilterByOrientation ? "displaced" :
					displacementReceived.RequiredOrientation == MoveStatus.Orientation.AwayFromCaster ? "pushed" : "pulled";
				return "Be " + displaceReceivedLabel + " " + displacementReceived.RequiredCount + " times " + duration;

			// ── Teleport ─────────────────────────────────────────────────────────
			case TeleportCountRequirement teleportCount:
				return "Teleport " + teleportCount.RequiredCount + " times " + duration;

			case TeleportDistanceRequirement teleportDistance:
				return "Teleport " + teleportDistance.RequiredDistance + " tiles " + duration;

			// ── Positional ───────────────────────────────────────────────────────
			case TurnStartPositionRequirement turnStart:
				return FormatPositionRequirement("Start turn", turnStart.Target, turnStart.Condition, turnStart.Distance, turnStart.MaximumDistance) + FormatRepeatCount(turnStart);

			case TurnEndPositionRequirement turnEnd:
				return FormatPositionRequirement("End turn", turnEnd.Target, turnEnd.Condition, turnEnd.Distance, turnEnd.MaximumDistance) + FormatRepeatCount(turnEnd);

			// ── Ability casting ──────────────────────────────────────────────────
			case CastAbilityCountRequirement castAbility:
				string abilitiesLabel = castAbility.Abilities.Count == 0 ? "any ability" : string.Join("/", castAbility.Abilities.ConvertAll(a => FormatAbilityName(a)));
				return "Cast " + abilitiesLabel + FormatTargetRangeCondition(castAbility) + " " + castAbility.RequiredCount + " times " + duration;

			// ── Meta ─────────────────────────────────────────────────────────────
			case AndRequirement andReq:
				return "All of " + (andReq.Children?.Count ?? 0) + " conditions";

			case OrRequirement orReq:
				return "Any of " + (orReq.Children?.Count ?? 0) + " conditions";

			default:
				return GetFeatRequirementLabel(requirement.GetType()) + " " + duration;
		}
	}

	private static string FormatPositionRequirement(
		string prefix,
		TurnStartPositionRequirement.TargetKind target,
		TurnStartPositionRequirement.DistanceKind condition,
		int distance,
		int maxDistance)
	{
		string targetLabel = target switch
		{
			TurnStartPositionRequirement.TargetKind.Ally => "an ally",
			TurnStartPositionRequirement.TargetKind.Enemy => "an enemy",
			_ => "any unit"
		};
		string conditionLabel = condition switch
		{
			TurnStartPositionRequirement.DistanceKind.Within => "within " + distance + " tiles of",
			TurnStartPositionRequirement.DistanceKind.AtLeast => "at least " + distance + " tiles from",
			TurnStartPositionRequirement.DistanceKind.Between => "between " + System.Math.Min(distance, maxDistance) + " and " + System.Math.Max(distance, maxDistance) + " tiles from",
			_ => string.Empty
		};
		return prefix + " " + conditionLabel + " " + targetLabel;
	}

	private static string FormatPositionRequirement(
		string prefix,
		TurnEndPositionRequirement.TargetKind target,
		TurnEndPositionRequirement.DistanceKind condition,
		int distance,
		int maxDistance)
	{
		string targetLabel = target switch
		{
			TurnEndPositionRequirement.TargetKind.Ally => "an ally",
			TurnEndPositionRequirement.TargetKind.Enemy => "an enemy",
			_ => "any unit"
		};
		string conditionLabel = condition switch
		{
			TurnEndPositionRequirement.DistanceKind.Within => "within " + distance + " tiles of",
			TurnEndPositionRequirement.DistanceKind.AtLeast => "at least " + distance + " tiles from",
			TurnEndPositionRequirement.DistanceKind.Between => "between " + System.Math.Min(distance, maxDistance) + " and " + System.Math.Max(distance, maxDistance) + " tiles from",
			_ => string.Empty
		};
		return prefix + " " + conditionLabel + " " + targetLabel;
	}

	private static string FormatRepeatCount(FeatRequirement requirement)
	{
		if (requirement == null || requirement.RequiredRepeatCount <= 1)
		{
			return string.Empty;
		}

		return " x" + requirement.RequiredRepeatCount;
	}

	private static string FormatRequirementDuration(FeatRequirement requirement)
	{
		if (requirement == null)
		{
			return string.Empty;
		}

		string durationText = requirement.RequirementScope switch
		{
			FeatRequirement.Scope.Action => "in one action",
			FeatRequirement.Scope.Turn => "in one turn",
			FeatRequirement.Scope.Fight => "in one fight",
			FeatRequirement.Scope.Game => "over the game",
			_ => string.Empty
		};

		if (requirement.RequiredRepeatCount <= 1)
		{
			return durationText;
		}

		return durationText + " x" + requirement.RequiredRepeatCount;
	}

	private static string FormatAbilityName(Ability ability)
	{
		return ability != null ? ability.name : "any ability";
	}

	private static string FormatDamageKindFilter(DamageKindFilter filter)
	{
		return filter == DamageKindFilter.Any ? string.Empty : " " + filter.ToString().ToLowerInvariant();
	}

	private static string FormatTargetRangeCondition(CastAbilityCountRequirement requirement)
	{
		if (requirement == null ||
			requirement.TargetRangeCondition == CastAbilityCountRequirement.RangeCondition.Either)
		{
			return string.Empty;
		}

		return requirement.TargetRangeCondition switch
		{
			CastAbilityCountRequirement.RangeCondition.AtLeast => " at range " + requirement.Range + "+",
			CastAbilityCountRequirement.RangeCondition.Within => " within range " + requirement.Range,
			_ => string.Empty
		};
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
