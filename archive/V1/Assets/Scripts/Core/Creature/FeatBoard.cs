using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature
{
	public enum FeatObjectiveKind
	{
		None = 0,
		DealDamage = 1,
		RestoreHealth = 2,
		EndTurnAwayFromEnemy = 3
	}

	public enum FeatRewardKind
	{
		None = 0,
		StatBonus = 1,
		AttackUnlock = 2
	}

	[Serializable]
	public sealed class FeatReward
	{
		[SerializeField] private FeatRewardKind kind = FeatRewardKind.None;
		[SerializeField] private Erelia.Core.Creature.Stats statBonus = new Erelia.Core.Creature.Stats();
		[SerializeField] private Erelia.Battle.Attack attack;
		[SerializeField] private string summary;

		public FeatRewardKind Kind => kind;
		public Erelia.Core.Creature.Stats StatBonus => statBonus ??= new Erelia.Core.Creature.Stats();
		public Erelia.Battle.Attack Attack => attack;
		public string Summary => summary;

		public string BuildSummary()
		{
			if (!string.IsNullOrWhiteSpace(summary))
			{
				return summary;
			}

			switch (kind)
			{
				case FeatRewardKind.StatBonus:
					return BuildStatBonusSummary(StatBonus);

				case FeatRewardKind.AttackUnlock:
					return attack != null ? $"Unlock {attack.DisplayName}" : "Unlock attack";

				default:
					return "No reward";
			}
		}

		private static string BuildStatBonusSummary(Erelia.Core.Creature.Stats stats)
		{
			if (stats == null)
			{
				return "Stat bonus";
			}

			var parts = new List<string>(4);
			if (stats.Health != 0)
			{
				parts.Add($"+{stats.Health} HP");
			}

			if (stats.Strength != 0)
			{
				parts.Add($"+{stats.Strength} Strength");
			}

			if (stats.Ability != 0)
			{
				parts.Add($"+{stats.Ability} Ability");
			}

			if (stats.Armor != 0)
			{
				parts.Add($"+{stats.Armor} Armor");
			}

			if (stats.Resistance != 0)
			{
				parts.Add($"+{stats.Resistance} Resistance");
			}

			if (stats.ActionPoints != 0)
			{
				parts.Add($"+{stats.ActionPoints} AP");
			}

			if (stats.MovementPoints != 0)
			{
				parts.Add($"+{stats.MovementPoints} MP");
			}

			if (!Mathf.Approximately(stats.Stamina, 0f))
			{
				parts.Add($"+{stats.Stamina:0.##} Stamina");
			}

			if (stats.Range != 0)
			{
				parts.Add($"+{stats.Range} Range");
			}

			return parts.Count > 0 ? string.Join(", ", parts) : "Stat bonus";
		}
	}

	[Serializable]
	public sealed class FeatNode
	{
		[SerializeField] private string id;
		[SerializeField] private string displayName;
		[SerializeField, TextArea] private string description;
		[SerializeField] private Vector2 position;
		[SerializeField] private bool startsUnlocked;
		[SerializeField] private string[] adjacentNodeIds = Array.Empty<string>();
		[SerializeField] private FeatObjectiveKind objectiveKind = FeatObjectiveKind.None;
		[SerializeField] private int requiredValue = 1;
		[SerializeField] private int minimumDistanceToEnemy = 3;
		[SerializeField] private FeatReward reward = new FeatReward();

		public string Id => id;
		public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? id : displayName;
		public string Description => description;
		public Vector2 Position => position;
		public bool StartsUnlocked => startsUnlocked;
		public IReadOnlyList<string> AdjacentNodeIds => adjacentNodeIds ?? Array.Empty<string>();
		public FeatObjectiveKind ObjectiveKind => objectiveKind;
		public int RequiredValue => Mathf.Max(1, requiredValue);
		public int MinimumDistanceToEnemy => Mathf.Max(0, minimumDistanceToEnemy);
		public FeatReward Reward => reward ??= new FeatReward();
		public bool HasValidId => !string.IsNullOrWhiteSpace(id);

		public string BuildObjectiveSummary()
		{
			switch (objectiveKind)
			{
				case FeatObjectiveKind.DealDamage:
					return $"Deal {RequiredValue} damage";

				case FeatObjectiveKind.RestoreHealth:
					return $"Restore {RequiredValue} health";

				case FeatObjectiveKind.EndTurnAwayFromEnemy:
					return $"End {RequiredValue} turns {MinimumDistanceToEnemy}+ cells away";

				default:
					return "Starting node";
			}
		}
	}

	[CreateAssetMenu(menuName = "Creature/Feat Board", fileName = "NewFeatBoard")]
	public sealed class FeatBoard : ScriptableObject
	{
		[SerializeField] private List<FeatNode> nodes = new List<FeatNode>();

		[NonSerialized] private Dictionary<string, FeatNode> nodeLookup;
		[NonSerialized] private Dictionary<string, HashSet<string>> adjacencyLookup;

		public IReadOnlyList<FeatNode> Nodes => nodes;

		public bool TryGetNode(string nodeId, out FeatNode node)
		{
			node = null;
			BuildCache();
			return nodeLookup != null &&
				!string.IsNullOrWhiteSpace(nodeId) &&
				nodeLookup.TryGetValue(nodeId, out node);
		}

		public List<FeatNode> GetStartingNodes()
		{
			BuildCache();
			var startingNodes = new List<FeatNode>();
			if (nodes == null)
			{
				return startingNodes;
			}

			for (int i = 0; i < nodes.Count; i++)
			{
				FeatNode node = nodes[i];
				if (node != null && node.HasValidId && node.StartsUnlocked)
				{
					startingNodes.Add(node);
				}
			}

			return startingNodes;
		}

		public List<FeatNode> GetActiveNodes(Erelia.Core.Creature.FeatProgress progress)
		{
			BuildCache();
			var activeNodes = new List<FeatNode>();
			if (progress == null || nodes == null)
			{
				return activeNodes;
			}

			var addedIds = new HashSet<string>(StringComparer.Ordinal);
			for (int i = 0; i < nodes.Count; i++)
			{
				FeatNode node = nodes[i];
				if (node == null || !node.HasValidId || progress.IsCompleted(node.Id))
				{
					continue;
				}

				if (!node.StartsUnlocked && !IsAdjacentToCompletedNode(node.Id, progress))
				{
					continue;
				}

				if (addedIds.Add(node.Id))
				{
					activeNodes.Add(node);
				}
			}

			return activeNodes;
		}

		private void OnEnable()
		{
			ClearCache();
		}

		private void OnValidate()
		{
			ClearCache();
		}

		private void ClearCache()
		{
			nodeLookup = null;
			adjacencyLookup = null;
		}

		private void BuildCache()
		{
			if (nodeLookup != null && adjacencyLookup != null)
			{
				return;
			}

			nodeLookup = new Dictionary<string, FeatNode>(StringComparer.Ordinal);
			adjacencyLookup = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
			if (nodes == null)
			{
				return;
			}

			for (int i = 0; i < nodes.Count; i++)
			{
				FeatNode node = nodes[i];
				if (node == null || !node.HasValidId)
				{
					continue;
				}

				if (!nodeLookup.ContainsKey(node.Id))
				{
					nodeLookup.Add(node.Id, node);
					adjacencyLookup.Add(node.Id, new HashSet<string>(StringComparer.Ordinal));
					continue;
				}

				Debug.LogWarning($"[Erelia.Core.Creature.FeatBoard] Duplicate feat node id '{node.Id}' on board '{name}'.");
			}

			for (int i = 0; i < nodes.Count; i++)
			{
				FeatNode node = nodes[i];
				if (node == null || !node.HasValidId || !adjacencyLookup.TryGetValue(node.Id, out HashSet<string> neighbors))
				{
					continue;
				}

				IReadOnlyList<string> adjacentIds = node.AdjacentNodeIds;
				for (int adjacentIndex = 0; adjacentIndex < adjacentIds.Count; adjacentIndex++)
				{
					string adjacentId = adjacentIds[adjacentIndex];
					if (string.IsNullOrWhiteSpace(adjacentId) || !adjacencyLookup.ContainsKey(adjacentId))
					{
						continue;
					}

					neighbors.Add(adjacentId);
					adjacencyLookup[adjacentId].Add(node.Id);
				}
			}
		}

		private bool IsAdjacentToCompletedNode(string nodeId, Erelia.Core.Creature.FeatProgress progress)
		{
			if (progress == null ||
				string.IsNullOrWhiteSpace(nodeId) ||
				adjacencyLookup == null ||
				!adjacencyLookup.TryGetValue(nodeId, out HashSet<string> neighbors) ||
				neighbors == null ||
				neighbors.Count == 0)
			{
				return false;
			}

			IReadOnlyList<Erelia.Core.Creature.FeatNodeProgress> states = progress.Nodes;
			for (int i = 0; i < states.Count; i++)
			{
				Erelia.Core.Creature.FeatNodeProgress state = states[i];
				if (state != null && state.Completed && neighbors.Contains(state.NodeId))
				{
					return true;
				}
			}

			return false;
		}
	}
}


