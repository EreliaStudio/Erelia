using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using GameStatus = Status;

namespace Tests.Requirements.MaxSingleHit
{
	public sealed class MaxSingleHitTests
	{
		private static BattleUnit CreateTestUnit() => new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<global::Status>() },
			BattleSide.Player);

		[Test]
		public void TwoWeakHits_DoNotProgress()
		{
			var requirement = new DealDamageRequirement { RequiredAmount = 100, RequirementScope = FeatRequirement.Scope.Action };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DamageEvent { Amount = 30, Caster = CreateTestUnit() } });
			progress.RegisterEvents(new[] { new DamageEvent { Amount = 30, Caster = CreateTestUnit() } });

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void HitAtThreshold_Completes()
		{
			var requirement = new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Action };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DamageEvent { Amount = 50, Caster = CreateTestUnit() } });

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void SingleEventBelowThreshold_DoesNotComplete()
		{
			var requirement = new DealDamageRequirement { RequiredAmount = 100, RequirementScope = FeatRequirement.Scope.Action };
			var progress = new FeatRequirementProgress { Requirement = requirement };

			progress.RegisterEvents(new[] { new DamageEvent { Amount = 40, Caster = CreateTestUnit() } });
			progress.RegisterEvents(new[] { new DamageEvent { Amount = 40, Caster = CreateTestUnit() } });

			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(0));
			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void TwoWeakHits_DoNotCompleteNode()
		{
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Action }
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
			FeatBoardService.InitializeCreatureUnit(creatureUnit);

			FeatBoardService.RegisterEvent(creatureUnit, new DamageEvent { Amount = 25, Caster = CreateTestUnit() });
			FeatBoardService.RegisterEvent(creatureUnit, new DamageEvent { Amount = 25, Caster = CreateTestUnit() });

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(creatureUnit, maxHitNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False, "Two weak hits must not satisfy a max-single-hit requirement.");

			Object.DestroyImmediate(creatureUnit.Species);
		}

		[Test]
		public void SingleHitAtThreshold_CompletesNode()
		{
			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Action }
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
			FeatBoardService.InitializeCreatureUnit(creatureUnit);

			FeatBoardService.RegisterEvent(creatureUnit, new DamageEvent { Amount = 50, Caster = CreateTestUnit() });

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(creatureUnit, maxHitNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			Object.DestroyImmediate(creatureUnit.Species);
		}

		[Test]
		public void PlayerVictory_NodeCompleted_WhenSingleHitMeetsThreshold()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1, defaultHealth: 50, defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				DisplayName = "Heavy Hitter",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 30, RequirementScope = FeatRequirement.Scope.Action }
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
			FeatBoardService.InitializeCreatureUnit(fixture.PlayerSources[0]);
			using ServiceLocatorTestScope services = new ServiceLocatorTestScope();
			EventCenter.EmitBattleStarted(fixture.BattleContext);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);
			BattleEventReporter.Emit(new DamageEvent { Amount = 30, Caster = fixture.PlayerUnits[0] });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(fixture.PlayerSources[0], maxHitNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}

		[Test]
		public void PlayerVictory_NodeNotCompleted_WhenNoSingleHitMeetsThreshold()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1, enemyCount: 1, defaultHealth: 50, defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode maxHitNode = new FeatNode
			{
				Id = "max_hit_node",
				DisplayName = "Heavy Hitter",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50, RequirementScope = FeatRequirement.Scope.Action }
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
			FeatBoardService.InitializeCreatureUnit(fixture.PlayerSources[0]);
			using ServiceLocatorTestScope services = new ServiceLocatorTestScope();
			EventCenter.EmitBattleStarted(fixture.BattleContext);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);
			BattleEventReporter.Emit(new DamageEvent { Amount = 25, Caster = fixture.PlayerUnits[0] });
			BattleEventReporter.Emit(new DamageEvent { Amount = 25, Caster = fixture.PlayerUnits[0] });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(fixture.PlayerSources[0], maxHitNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False, "Two hits of 25 must not complete a max-single-hit-50 node.");

			orchestrator.Dispose();
		}
	}
}
