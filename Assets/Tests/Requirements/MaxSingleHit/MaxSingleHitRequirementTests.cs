using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using GameStatus = Status;

namespace Tests.Requirements.MaxSingleHit
{
	public sealed class MaxSingleHitRequirementTests
	{
		[Test]
		public void TwoWeakHitsDoNotProgress()
		{
			var requirement = new DealDamageRequirement { RequiredAmount = 100, RequirementScope = FeatRequirement.Scope.Ability };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 30 } });
			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 30 } });

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void HitAtThresholdCompletes()
		{
			var requirement = new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Ability };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 50 } });

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void SingleEventBelowThresholdDoesNotComplete()
		{
			var requirement = new DealDamageRequirement
			{
				RequiredAmount = 100,
				RequirementScope = FeatRequirement.Scope.Ability
			};
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 40 } });
			progress.RegisterEvents(new[] { new DealDamageRequirement.Event { Amount = 40 } });

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void TwoWeakHitsDoNotCompleteNode()
		{
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Ability }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};

			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<GameStatus>()
			};
			creatureUnit.Species = ScriptableObject.CreateInstance<CreatureSpecies>();
			creatureUnit.Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(creatureUnit);

			FeatProgressionService.RegisterEvent(creatureUnit, new DealDamageRequirement.Event { Amount = 25 });
			FeatProgressionService.RegisterEvent(creatureUnit, new DealDamageRequirement.Event { Amount = 25 });

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creatureUnit, maxHitNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False, "Two weak hits must not satisfy a max-single-hit requirement.");

			Object.DestroyImmediate(creatureUnit.Species);
		}

		[Test]
		public void SingleHitAtThresholdCompletesNode()
		{
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Ability }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};

			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes { Health = 100 },
				Abilities = new List<Ability>(),
				PermanentPassives = new List<GameStatus>()
			};
			creatureUnit.Species = ScriptableObject.CreateInstance<CreatureSpecies>();
			creatureUnit.Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(creatureUnit);

			FeatProgressionService.RegisterEvent(creatureUnit, new DealDamageRequirement.Event { Amount = 50 });

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(creatureUnit, maxHitNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0), "A single hit meeting the threshold must complete the node.");

			Object.DestroyImmediate(creatureUnit.Species);
		}

		[Test]
		public void PlayerVictory_NodeCompleted_WhenSingleHitMeetsThreshold()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				DisplayName = "Heavy Hitter",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 30, RequirementScope = FeatRequirement.Scope.Ability }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(maxHitNode.Id);

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			fixture.PlayerUnits[0].RecordFeatEvent(new DealDamageRequirement.Event { Amount = 30 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], maxHitNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}

		[Test]
		public void PlayerVictory_NodeNotCompleted_WhenNoSingleHitMeetsThreshold()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				DisplayName = "Heavy Hitter",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Ability }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(maxHitNode.Id);

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, maxHitNode },
				RootNodeId = rootNode.Id
			};
			FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			fixture.PlayerUnits[0].RecordFeatEvent(new DealDamageRequirement.Event { Amount = 25 });
			fixture.PlayerUnits[0].RecordFeatEvent(new DealDamageRequirement.Event { Amount = 25 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], maxHitNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False, "Two hits of 25 must not complete a max-single-hit-50 node.");

			orchestrator.Dispose();
		}
	}
}
