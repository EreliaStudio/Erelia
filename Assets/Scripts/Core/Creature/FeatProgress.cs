using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature
{
	[Serializable]
	public sealed class FeatNodeProgress
	{
		[SerializeField] private string nodeId;
		[SerializeField] private int currentValue;
		[SerializeField] private bool completed;
		[SerializeField] private bool rewardApplied;

		public FeatNodeProgress()
		{
		}

		public FeatNodeProgress(string nodeId)
		{
			this.nodeId = nodeId;
		}

		public string NodeId => nodeId;
		public int CurrentValue => Mathf.Max(0, currentValue);
		public bool Completed => completed;
		public bool RewardApplied => rewardApplied;
		public bool HasValidId => !string.IsNullOrWhiteSpace(nodeId);

		public bool TryAddProgress(int delta, int requiredValue, out bool completedThisCall)
		{
			completedThisCall = false;
			if (completed || delta <= 0)
			{
				return false;
			}

			int threshold = Mathf.Max(1, requiredValue);
			int previousValue = currentValue;
			currentValue = Mathf.Min(threshold, currentValue + delta);
			if (currentValue >= threshold)
			{
				completed = true;
				completedThisCall = true;
			}

			return currentValue != previousValue || completedThisCall;
		}

		public void MarkCompleted(int requiredValue, bool markRewardApplied)
		{
			currentValue = Mathf.Max(currentValue, Mathf.Max(1, requiredValue));
			completed = true;
			if (markRewardApplied)
			{
				rewardApplied = true;
			}
		}

		public bool TryMarkRewardApplied()
		{
			if (rewardApplied)
			{
				return false;
			}

			rewardApplied = true;
			return true;
		}
	}

	[Serializable]
	public sealed class FeatProgress
	{
		[SerializeField] private List<Erelia.Core.Creature.FeatNodeProgress> nodes =
			new List<Erelia.Core.Creature.FeatNodeProgress>();

		public IReadOnlyList<Erelia.Core.Creature.FeatNodeProgress> Nodes =>
			nodes ??= new List<Erelia.Core.Creature.FeatNodeProgress>();

		public void Normalize()
		{
			nodes ??= new List<Erelia.Core.Creature.FeatNodeProgress>();
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				if (nodes[i] == null || !nodes[i].HasValidId)
				{
					nodes.RemoveAt(i);
				}
			}
		}

		public Erelia.Core.Creature.FeatNodeProgress GetOrCreateNode(string nodeId)
		{
			if (string.IsNullOrWhiteSpace(nodeId))
			{
				return null;
			}

			Normalize();
			for (int i = 0; i < nodes.Count; i++)
			{
				Erelia.Core.Creature.FeatNodeProgress node = nodes[i];
				if (node != null && string.Equals(node.NodeId, nodeId, StringComparison.Ordinal))
				{
					return node;
				}
			}

			var createdNode = new Erelia.Core.Creature.FeatNodeProgress(nodeId);
			nodes.Add(createdNode);
			return createdNode;
		}

		public bool TryGetNode(string nodeId, out Erelia.Core.Creature.FeatNodeProgress node)
		{
			node = null;
			if (string.IsNullOrWhiteSpace(nodeId))
			{
				return false;
			}

			Normalize();
			for (int i = 0; i < nodes.Count; i++)
			{
				Erelia.Core.Creature.FeatNodeProgress candidate = nodes[i];
				if (candidate != null && string.Equals(candidate.NodeId, nodeId, StringComparison.Ordinal))
				{
					node = candidate;
					return true;
				}
			}

			return false;
		}

		public bool IsCompleted(string nodeId)
		{
			return TryGetNode(nodeId, out Erelia.Core.Creature.FeatNodeProgress node) && node.Completed;
		}

		public bool TryAddProgress(
			string nodeId,
			int delta,
			int requiredValue,
			out bool completedThisCall,
			out int currentValue)
		{
			completedThisCall = false;
			currentValue = 0;

			Erelia.Core.Creature.FeatNodeProgress node = GetOrCreateNode(nodeId);
			if (node == null || !node.TryAddProgress(delta, requiredValue, out completedThisCall))
			{
				currentValue = node != null ? node.CurrentValue : 0;
				return false;
			}

			currentValue = node.CurrentValue;
			return true;
		}

		public void EnsureCompleted(string nodeId, int requiredValue, bool rewardApplied)
		{
			Erelia.Core.Creature.FeatNodeProgress node = GetOrCreateNode(nodeId);
			node?.MarkCompleted(requiredValue, rewardApplied);
		}
	}

	public static class FeatProgression
	{
		public static Erelia.Core.Creature.FeatBoard GetBoard(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null || creature.IsEmpty)
			{
				return null;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry == null ||
				!registry.TryGet(creature.SpeciesId, out Erelia.Core.Creature.Species species) ||
				species == null)
			{
				return null;
			}

			return species.FeatBoard;
		}

		public static void EnsureInitialized(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null || creature.IsEmpty)
			{
				return;
			}

			Erelia.Core.Creature.FeatProgress progress = creature.FeatProgress;
			progress.Normalize();

			Erelia.Core.Creature.FeatBoard board = GetBoard(creature);
			if (board == null)
			{
				return;
			}

			List<Erelia.Core.Creature.FeatNode> startingNodes = board.GetStartingNodes();
			for (int i = 0; i < startingNodes.Count; i++)
			{
				Erelia.Core.Creature.FeatNode node = startingNodes[i];
				if (node != null && node.HasValidId)
				{
					progress.EnsureCompleted(node.Id, node.RequiredValue, true);
				}
			}
		}

		public static bool IsBoardComplete(Erelia.Core.Creature.Instance.Model creature)
		{
			EnsureInitialized(creature);

			Erelia.Core.Creature.FeatBoard board = GetBoard(creature);
			if (board == null || board.Nodes == null || board.Nodes.Count == 0)
			{
				return false;
			}

			for (int i = 0; i < board.Nodes.Count; i++)
			{
				Erelia.Core.Creature.FeatNode node = board.Nodes[i];
				if (node != null && node.HasValidId && !creature.FeatProgress.IsCompleted(node.Id))
				{
					return false;
				}
			}

			return true;
		}

		public static bool TryGetPrimaryActiveNode(
			Erelia.Core.Creature.Instance.Model creature,
			out Erelia.Core.Creature.FeatNode node,
			out Erelia.Core.Creature.FeatNodeProgress progress)
		{
			node = null;
			progress = null;

			EnsureInitialized(creature);
			Erelia.Core.Creature.FeatBoard board = GetBoard(creature);
			if (board == null)
			{
				return false;
			}

			List<Erelia.Core.Creature.FeatNode> activeNodes = board.GetActiveNodes(creature.FeatProgress);
			if (activeNodes.Count == 0)
			{
				return false;
			}

			node = activeNodes[0];
			creature.FeatProgress.TryGetNode(node.Id, out progress);
			return true;
		}

		public static int RegisterDamageDealt(
			Erelia.Core.Creature.Instance.Model creature,
			int amount,
			List<Erelia.Core.Creature.FeatNode> impactedNodes = null,
			List<Erelia.Core.Creature.FeatNode> completedNodes = null)
		{
			return RegisterProgress(
				creature,
				Erelia.Core.Creature.FeatObjectiveKind.DealDamage,
				amount,
				0,
				impactedNodes,
				completedNodes);
		}

		public static int RegisterHealingDone(
			Erelia.Core.Creature.Instance.Model creature,
			int amount,
			List<Erelia.Core.Creature.FeatNode> impactedNodes = null,
			List<Erelia.Core.Creature.FeatNode> completedNodes = null)
		{
			return RegisterProgress(
				creature,
				Erelia.Core.Creature.FeatObjectiveKind.RestoreHealth,
				amount,
				0,
				impactedNodes,
				completedNodes);
		}

		public static int RegisterTurnEndedAwayFromEnemies(
			Erelia.Core.Creature.Instance.Model creature,
			int closestEnemyDistance,
			List<Erelia.Core.Creature.FeatNode> impactedNodes = null,
			List<Erelia.Core.Creature.FeatNode> completedNodes = null)
		{
			if (closestEnemyDistance < 0)
			{
				return 0;
			}

			return RegisterProgress(
				creature,
				Erelia.Core.Creature.FeatObjectiveKind.EndTurnAwayFromEnemy,
				1,
				closestEnemyDistance,
				impactedNodes,
				completedNodes);
		}

		public static bool TryApplyReward(
			Erelia.Core.Creature.Instance.Model creature,
			Erelia.Core.Creature.FeatNode node,
			out string rewardSummary)
		{
			rewardSummary = string.Empty;
			if (creature == null || node == null || !node.HasValidId)
			{
				return false;
			}

			EnsureInitialized(creature);
			if (!creature.FeatProgress.TryGetNode(node.Id, out Erelia.Core.Creature.FeatNodeProgress progressNode) ||
				!progressNode.Completed)
			{
				return false;
			}

			if (progressNode.RewardApplied)
			{
				return false;
			}

			Erelia.Core.Creature.FeatReward reward = node.Reward;
			switch (reward.Kind)
			{
				case Erelia.Core.Creature.FeatRewardKind.None:
					progressNode.TryMarkRewardApplied();
					rewardSummary = reward.BuildSummary();
					return true;

				case Erelia.Core.Creature.FeatRewardKind.StatBonus:
					creature.ApplyStatBonus(reward.StatBonus);
					progressNode.TryMarkRewardApplied();
					rewardSummary = reward.BuildSummary();
					return true;

				case Erelia.Core.Creature.FeatRewardKind.AttackUnlock:
					if (reward.Attack == null)
					{
						progressNode.TryMarkRewardApplied();
						rewardSummary = reward.BuildSummary();
						return true;
					}

					if (creature.TryUnlockAttack(reward.Attack))
					{
						progressNode.TryMarkRewardApplied();
						rewardSummary = reward.BuildSummary();
						return true;
					}

					rewardSummary = $"Pending attack reward: {reward.Attack.DisplayName}";
					Debug.LogWarning(
						$"[Erelia.Core.Creature.FeatProgression] Could not grant attack '{reward.Attack.DisplayName}' to '{creature.DisplayName}' because all attack slots are occupied.");
					return false;

				default:
					return false;
			}
		}

		public static string BuildProgressSummary(
			Erelia.Core.Creature.FeatNode node,
			Erelia.Core.Creature.FeatNodeProgress progress)
		{
			if (node == null)
			{
				return string.Empty;
			}

			int currentValue = progress != null ? Mathf.Min(progress.CurrentValue, node.RequiredValue) : 0;
			return $"{node.BuildObjectiveSummary()} ({currentValue}/{node.RequiredValue})";
		}

		public static float GetProgressRatio(
			Erelia.Core.Creature.FeatNode node,
			Erelia.Core.Creature.FeatNodeProgress progress)
		{
			if (node == null)
			{
				return 0f;
			}

			int requiredValue = Mathf.Max(1, node.RequiredValue);
			int currentValue = progress != null ? Mathf.Min(progress.CurrentValue, requiredValue) : 0;
			return currentValue / (float)requiredValue;
		}

		private static int RegisterProgress(
			Erelia.Core.Creature.Instance.Model creature,
			Erelia.Core.Creature.FeatObjectiveKind objectiveKind,
			int amount,
			int closestEnemyDistance,
			List<Erelia.Core.Creature.FeatNode> impactedNodes,
			List<Erelia.Core.Creature.FeatNode> completedNodes)
		{
			if (creature == null || creature.IsEmpty || amount <= 0)
			{
				return 0;
			}

			EnsureInitialized(creature);
			Erelia.Core.Creature.FeatBoard board = GetBoard(creature);
			if (board == null)
			{
				return 0;
			}

			List<Erelia.Core.Creature.FeatNode> activeNodes = board.GetActiveNodes(creature.FeatProgress);
			int completedCount = 0;
			for (int i = 0; i < activeNodes.Count; i++)
			{
				Erelia.Core.Creature.FeatNode node = activeNodes[i];
				if (node == null ||
					!node.HasValidId ||
					node.ObjectiveKind != objectiveKind)
				{
					continue;
				}

				if (objectiveKind == Erelia.Core.Creature.FeatObjectiveKind.EndTurnAwayFromEnemy &&
					closestEnemyDistance < node.MinimumDistanceToEnemy)
				{
					continue;
				}

				if (creature.FeatProgress.TryAddProgress(
						node.Id,
						amount,
						node.RequiredValue,
						out bool completedThisCall,
						out int currentValue))
				{
					impactedNodes?.Add(node);
					if (completedThisCall)
					{
						completedNodes?.Add(node);
						completedCount++;
					}
				}
			}

			return completedCount;
		}
	}
}
