using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[Serializable]
	public sealed class FeatProgressTracker
	{
		private sealed class ImpactedFeatNodeState
		{
			public ImpactedFeatNodeState(Erelia.Core.Creature.FeatNode node)
			{
				Node = node;
			}

			public Erelia.Core.Creature.FeatNode Node { get; set; }
			public bool CompletedThisBattle { get; set; }
			public string RewardSummary { get; set; }
		}

		private sealed class CreatureBattleState
		{
			public CreatureBattleState(Erelia.Core.Creature.Instance.CreatureInstance creature)
			{
				Creature = creature;
			}

			public Erelia.Core.Creature.Instance.CreatureInstance Creature { get; }
			public readonly List<ImpactedFeatNodeState> ImpactedNodes =
				new List<ImpactedFeatNodeState>();
			public readonly Dictionary<string, ImpactedFeatNodeState> ImpactedNodeLookup =
				new Dictionary<string, ImpactedFeatNodeState>(StringComparer.Ordinal);
			public readonly HashSet<string> CompletedNodeIds =
				new HashSet<string>(StringComparer.Ordinal);
			public bool RewardsResolved { get; set; }
		}

		[NonSerialized] private readonly Dictionary<Erelia.Core.Creature.Instance.CreatureInstance, CreatureBattleState> stateByCreature =
			new Dictionary<Erelia.Core.Creature.Instance.CreatureInstance, CreatureBattleState>();
		[NonSerialized] private readonly List<Erelia.Battle.BattleResultEntry> resultEntries =
			new List<Erelia.Battle.BattleResultEntry>();
		[NonSerialized] private readonly List<Erelia.Battle.BattleResultCreature> creatureResults =
			new List<Erelia.Battle.BattleResultCreature>();

		public IReadOnlyList<Erelia.Battle.BattleResultEntry> ResultEntries => resultEntries;
		public IReadOnlyList<Erelia.Battle.BattleResultCreature> CreatureResults => creatureResults;

		public void Reset()
		{
			stateByCreature.Clear();
			resultEntries.Clear();
			creatureResults.Clear();
		}

		public void BeginBattle(IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits)
		{
			Reset();
			if (playerUnits == null)
			{
				return;
			}

			for (int i = 0; i < playerUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = playerUnits[i];
				if (unit?.Creature == null)
				{
					continue;
				}

				GetOrCreateState(unit.Creature);
				Erelia.Core.Creature.FeatProgression.EnsureInitialized(unit.Creature);
			}
		}

		public void RegisterHealthChange(
			Erelia.Battle.Unit.Presenter actor,
			Erelia.Battle.Unit.Presenter target,
			int previousHealth,
			int currentHealth)
		{
			if (actor == null ||
				target == null ||
				actor.Creature == null ||
				previousHealth == currentHealth)
			{
				return;
			}

			CreatureBattleState state = GetTrackedState(actor.Creature);
			if (state == null)
			{
				return;
			}

			var impactedNodes = new List<Erelia.Core.Creature.FeatNode>();
			var completedNodes = new List<Erelia.Core.Creature.FeatNode>();
			if (previousHealth > currentHealth && actor.Side != target.Side)
			{
				Erelia.Core.Creature.FeatProgression.RegisterDamageDealt(
					actor.Creature,
					previousHealth - currentHealth,
					impactedNodes,
					completedNodes);
			}
			else if (currentHealth > previousHealth && actor.Side == target.Side)
			{
				Erelia.Core.Creature.FeatProgression.RegisterHealingDone(
					actor.Creature,
					currentHealth - previousHealth,
					impactedNodes,
					completedNodes);
			}

			RegisterImpactedNodes(state, impactedNodes, completedNodes);
		}

		public void RegisterTurnEnded(
			Erelia.Battle.Unit.Presenter actor,
			IReadOnlyList<Erelia.Battle.Unit.Presenter> allUnits)
		{
			if (actor == null || actor.Creature == null)
			{
				return;
			}

			CreatureBattleState state = GetTrackedState(actor.Creature);
			if (state == null)
			{
				return;
			}

			int closestEnemyDistance = ResolveClosestEnemyDistance(actor, allUnits);
			if (closestEnemyDistance < 0)
			{
				return;
			}

			var impactedNodes = new List<Erelia.Core.Creature.FeatNode>();
			var completedNodes = new List<Erelia.Core.Creature.FeatNode>();
			Erelia.Core.Creature.FeatProgression.RegisterTurnEndedAwayFromEnemies(
				actor.Creature,
				closestEnemyDistance,
				impactedNodes,
				completedNodes);
			RegisterImpactedNodes(state, impactedNodes, completedNodes);
		}

		public void FinalizeBattle(
			Erelia.Core.Creature.Team playerTeam,
			IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits)
		{
			resultEntries.Clear();
			creatureResults.Clear();

			Erelia.Core.Creature.Instance.CreatureInstance[] slots = playerTeam?.Slots;
			if (HasAnyCreatureSlot(slots))
			{
				int slotCount = Mathf.Max(Erelia.Core.Creature.Team.DefaultSize, slots.Length);
				for (int i = 0; i < slotCount; i++)
				{
					Erelia.Core.Creature.Instance.CreatureInstance creature = i < slots.Length ? slots[i] : null;
					creatureResults.Add(BuildCreatureResult(i, creature, playerUnits));
				}

				return;
			}

			for (int i = 0; i < Erelia.Core.Creature.Team.DefaultSize; i++)
			{
				Erelia.Battle.Unit.Presenter unit = ResolvePlayerUnitAtSlot(playerUnits, i);
				creatureResults.Add(BuildCreatureResult(i, unit?.Creature, playerUnits));
			}
		}

		private static bool HasAnyCreatureSlot(Erelia.Core.Creature.Instance.CreatureInstance[] slots)
		{
			if (slots == null || slots.Length == 0)
			{
				return false;
			}

			for (int i = 0; i < slots.Length; i++)
			{
				Erelia.Core.Creature.Instance.CreatureInstance creature = slots[i];
				if (creature != null && !creature.IsEmpty)
				{
					return true;
				}
			}

			return false;
		}

		private static Erelia.Battle.Unit.Presenter ResolvePlayerUnitAtSlot(
			IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits,
			int slotIndex)
		{
			if (playerUnits == null || slotIndex < 0)
			{
				return null;
			}

			int liveIndex = 0;
			for (int i = 0; i < playerUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = playerUnits[i];
				if (unit?.Creature == null || unit.Creature.IsEmpty)
				{
					continue;
				}

				if (liveIndex == slotIndex)
				{
					return unit;
				}

				liveIndex++;
			}

			return null;
		}

		private CreatureBattleState GetTrackedState(Erelia.Core.Creature.Instance.CreatureInstance creature)
		{
			if (creature == null || !stateByCreature.TryGetValue(creature, out CreatureBattleState state))
			{
				return null;
			}

			return state;
		}

		private CreatureBattleState GetOrCreateState(Erelia.Core.Creature.Instance.CreatureInstance creature)
		{
			if (creature == null)
			{
				return null;
			}

			if (!stateByCreature.TryGetValue(creature, out CreatureBattleState state))
			{
				state = new CreatureBattleState(creature);
				stateByCreature.Add(creature, state);
			}

			return state;
		}

		private void RegisterImpactedNodes(
			CreatureBattleState state,
			List<Erelia.Core.Creature.FeatNode> impactedNodes,
			List<Erelia.Core.Creature.FeatNode> completedNodes)
		{
			if (state == null)
			{
				return;
			}

			if (completedNodes != null)
			{
				for (int i = 0; i < completedNodes.Count; i++)
				{
					Erelia.Core.Creature.FeatNode completedNode = completedNodes[i];
					if (completedNode != null && completedNode.HasValidId)
					{
						state.CompletedNodeIds.Add(completedNode.Id);
					}
				}
			}

			if (impactedNodes == null || impactedNodes.Count == 0)
			{
				return;
			}

			state.RewardsResolved = false;
			for (int i = 0; i < impactedNodes.Count; i++)
			{
				Erelia.Core.Creature.FeatNode node = impactedNodes[i];
				if (node == null || !node.HasValidId)
				{
					continue;
				}

				ImpactedFeatNodeState impactedState = GetOrCreateImpactedNodeState(state, node);
				impactedState.CompletedThisBattle |= state.CompletedNodeIds.Contains(node.Id);
			}
		}

		private static ImpactedFeatNodeState GetOrCreateImpactedNodeState(
			CreatureBattleState state,
			Erelia.Core.Creature.FeatNode node)
		{
			if (state == null || node == null || !node.HasValidId)
			{
				return null;
			}

			if (!state.ImpactedNodeLookup.TryGetValue(node.Id, out ImpactedFeatNodeState impactedState))
			{
				impactedState = new ImpactedFeatNodeState(node);
				state.ImpactedNodeLookup.Add(node.Id, impactedState);
				state.ImpactedNodes.Add(impactedState);
				return impactedState;
			}

			impactedState.Node = node;
			return impactedState;
		}

		private void ResolvePendingRewards(CreatureBattleState state)
		{
			if (state == null || state.RewardsResolved)
			{
				return;
			}

			for (int i = 0; i < state.ImpactedNodes.Count; i++)
			{
				ImpactedFeatNodeState impactedState = state.ImpactedNodes[i];
				Erelia.Core.Creature.FeatNode node = impactedState?.Node;
				if (node == null || !impactedState.CompletedThisBattle)
				{
					continue;
				}

				if (Erelia.Core.Creature.FeatProgression.TryApplyReward(
						state.Creature,
						node,
						out string rewardSummary) &&
					!string.IsNullOrWhiteSpace(rewardSummary))
				{
					impactedState.RewardSummary = rewardSummary;
					continue;
				}

				if (!string.IsNullOrWhiteSpace(rewardSummary))
				{
					impactedState.RewardSummary = rewardSummary;
				}
			}

			state.RewardsResolved = true;
		}

		private Erelia.Battle.BattleResultCreature BuildCreatureResult(
			int slotIndex,
			Erelia.Core.Creature.Instance.CreatureInstance creature,
			IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits)
		{
			if (creature == null || creature.IsEmpty)
			{
				return BuildEmptyCreatureResult(slotIndex);
			}

			Erelia.Battle.Unit.Presenter unit = FindPresenterForCreature(playerUnits, creature);
			CreatureBattleState state = GetOrCreateState(creature);
			ResolvePendingRewards(state);

			var entries = new List<Erelia.Battle.BattleResultEntry>();
			if (state != null)
			{
				for (int i = 0; i < state.ImpactedNodes.Count; i++)
				{
					ImpactedFeatNodeState impactedState = state.ImpactedNodes[i];
					if (impactedState?.Node == null)
					{
						continue;
					}

					Erelia.Battle.BattleResultEntry entry = BuildResultEntry(unit, creature, impactedState);
					entries.Add(entry);
					resultEntries.Add(entry);
				}
			}

			if (entries.Count == 0)
			{
				Erelia.Battle.BattleResultEntry emptyEntry = BuildNoProgressEntry();
				entries.Add(emptyEntry);
				resultEntries.Add(emptyEntry);
			}

			return new Erelia.Battle.BattleResultCreature(
				slotIndex,
				ResolveCreatureName(unit, creature, slotIndex),
				ResolveCreatureIcon(creature),
				true,
				entries.ToArray());
		}

		private static Erelia.Battle.BattleResultCreature BuildEmptyCreatureResult(int slotIndex)
		{
			return new Erelia.Battle.BattleResultCreature(
				slotIndex,
				string.Empty,
				null,
				false,
				Array.Empty<Erelia.Battle.BattleResultEntry>());
		}

		private static Erelia.Battle.BattleResultEntry BuildResultEntry(
			Erelia.Battle.Unit.Presenter unit,
			Erelia.Core.Creature.Instance.CreatureInstance creature,
			ImpactedFeatNodeState impactedState)
		{
			Erelia.Core.Creature.FeatNode node = impactedState?.Node;
			if (node == null)
			{
				return new Erelia.Battle.BattleResultEntry(
					Color.white,
					"Unknown Feat",
					"Unknown feat impact",
					0f,
					string.Empty);
			}

			Erelia.Core.Creature.FeatNodeProgress progress = null;
			creature?.FeatProgress.TryGetNode(node.Id, out progress);
			float progress01 = Erelia.Core.Creature.FeatProgression.GetProgressRatio(node, progress);
			int currentValue = progress != null ? Mathf.Min(progress.CurrentValue, node.RequiredValue) : 0;

			string objectiveSummary = node.BuildObjectiveSummary();
			string description = string.IsNullOrWhiteSpace(node.Description)
				? objectiveSummary
				: $"{node.Description} Objective: {objectiveSummary}.";
			if (!string.IsNullOrWhiteSpace(impactedState.RewardSummary))
			{
				description = $"{description} Reward: {impactedState.RewardSummary}.";
			}

			string progressLabel = impactedState.CompletedThisBattle
				? $"Completed {currentValue}/{node.RequiredValue}"
				: $"{currentValue}/{node.RequiredValue}";

			return new Erelia.Battle.BattleResultEntry(
				ResolveAccentColor(node, impactedState.CompletedThisBattle),
				node.DisplayName,
				description,
				progress01,
				progressLabel);
		}

		private static Erelia.Battle.BattleResultEntry BuildNoProgressEntry()
		{
			return new Erelia.Battle.BattleResultEntry(
				new Color(0.62f, 0.67f, 0.75f, 1f),
				"No Feat Progress",
				"No active feat node gained progress for this creature during the fight.",
				0f,
				string.Empty);
		}

		private static Color ResolveAccentColor(
			Erelia.Core.Creature.FeatNode node,
			bool isCompleted)
		{
			Color accent;
			switch (node != null ? node.ObjectiveKind : Erelia.Core.Creature.FeatObjectiveKind.None)
			{
				case Erelia.Core.Creature.FeatObjectiveKind.DealDamage:
					accent = new Color(0.93f, 0.35f, 0.33f, 1f);
					break;

				case Erelia.Core.Creature.FeatObjectiveKind.RestoreHealth:
					accent = new Color(0.31f, 0.79f, 0.5f, 1f);
					break;

				case Erelia.Core.Creature.FeatObjectiveKind.EndTurnAwayFromEnemy:
					accent = new Color(0.33f, 0.67f, 0.96f, 1f);
					break;

				default:
					accent = new Color(0.95f, 0.85f, 0.43f, 1f);
					break;
			}

			float variation = (ComputeStable01(node?.Id) - 0.5f) * 0.18f;
			accent = new Color(
				Mathf.Clamp01(accent.r + variation),
				Mathf.Clamp01(accent.g - variation * 0.4f),
				Mathf.Clamp01(accent.b + variation * 0.6f),
				1f);

			if (!isCompleted)
			{
				return accent;
			}

			return new Color(
				Mathf.Clamp01(accent.r + 0.12f),
				Mathf.Clamp01(accent.g + 0.12f),
				Mathf.Clamp01(accent.b + 0.12f),
				1f);
		}

		private static float ComputeStable01(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return 0.5f;
			}

			unchecked
			{
				int hash = 17;
				for (int i = 0; i < value.Length; i++)
				{
					hash = (hash * 31) + value[i];
				}

				return (hash & 1023) / 1023f;
			}
		}

		private static int ResolveClosestEnemyDistance(
			Erelia.Battle.Unit.Presenter actor,
			IReadOnlyList<Erelia.Battle.Unit.Presenter> allUnits)
		{
			if (actor == null || !actor.IsPlaced || allUnits == null)
			{
				return -1;
			}

			int closestDistance = int.MaxValue;
			for (int i = 0; i < allUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter candidate = allUnits[i];
				if (candidate == null ||
					!candidate.IsAlive ||
					!candidate.IsPlaced ||
					candidate.Side == actor.Side)
				{
					continue;
				}

				int distance =
					Mathf.Abs(candidate.Cell.x - actor.Cell.x) +
					Mathf.Abs(candidate.Cell.z - actor.Cell.z);
				if (distance < closestDistance)
				{
					closestDistance = distance;
				}
			}

			return closestDistance == int.MaxValue ? -1 : closestDistance;
		}

		private static Erelia.Battle.Unit.Presenter FindPresenterForCreature(
			IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits,
			Erelia.Core.Creature.Instance.CreatureInstance creature)
		{
			if (playerUnits == null || creature == null)
			{
				return null;
			}

			for (int i = 0; i < playerUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = playerUnits[i];
				if (unit != null && ReferenceEquals(unit.Creature, creature))
				{
					return unit;
				}
			}

			return null;
		}

		private static Sprite ResolveCreatureIcon(Erelia.Core.Creature.Instance.CreatureInstance creature)
		{
			return creature != null && !creature.IsEmpty ? creature.Icon : null;
		}

		private static string ResolveCreatureName(
			Erelia.Battle.Unit.Presenter unit,
			Erelia.Core.Creature.Instance.CreatureInstance creature,
			int slotIndex)
		{
			if (!string.IsNullOrEmpty(creature?.DisplayName))
			{
				return creature.DisplayName;
			}

			if (!string.IsNullOrEmpty(unit?.Creature?.DisplayName))
			{
				return unit.Creature.DisplayName;
			}

			if (!string.IsNullOrEmpty(unit?.name))
			{
				return unit.name;
			}

			return $"Creature {slotIndex + 1}";
		}
	}
}

