using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Feats.VictoryProgression
{
	public sealed class VictoryProgressionTests
	{
		[Test]
		public void PlayerVictory_FeatEventsAppliedToPlayerCreatureUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode damageNode = new FeatNode
			{
				Id = "deal_damage_node",
				DisplayName = "Damage Dealer",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50 }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 10 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(damageNode.Id);

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, damageNode },
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

			BattleFeatEventReporter.Emit(
				fixture.PlayerUnits[0],
				new DealDamageRequirement.Event { Amount = 50 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(fixture.PlayerSources[0], damageNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}

		[Test]
		public void EnemyVictory_FeatEventsStillAppliedToPlayerCreatureUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode damageNode = new FeatNode
			{
				Id = "deal_damage_node",
				DisplayName = "Damage Dealer",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50 }
				},
				Rewards = new List<FeatReward>
				{
					new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 10 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(damageNode.Id);

			fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, damageNode },
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

			fixture.PlayerUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.PlayerUnits[0]);

			BattleFeatEventReporter.Emit(
				fixture.PlayerUnits[0],
				new DealDamageRequirement.Event { Amount = 50 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(fixture.PlayerSources[0], damageNode);
			bool wasCompleted = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(wasCompleted, Is.True);

			orchestrator.Dispose();
		}

		[Test]
		public void PlayerVictory_FeatEventsNotAppliedToEnemyCreatureUnit()
		{
			using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
				playerCount: 1,
				enemyCount: 1,
				defaultHealth: 50,
				defaultActionPoints: 4);

			FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
			FeatNode damageNode = new FeatNode
			{
				Id = "enemy_damage_node",
				DisplayName = "Enemy Damage",
				Requirements = new List<FeatRequirement>
				{
					new DealDamageRequirement { RequiredAmount = 50 }
				},
				NeighbourNodeIds = new List<string> { rootNode.Id }
			};
			rootNode.NeighbourNodeIds.Add(damageNode.Id);

			fixture.EnemySources[0].Species.FeatBoard = new FeatBoard
			{
				Nodes = new List<FeatNode> { rootNode, damageNode },
				RootNodeId = rootNode.Id
			};
			FeatBoardService.InitializeCreatureUnit(fixture.EnemySources[0]);
			using ServiceLocatorTestScope services = new ServiceLocatorTestScope();
			EventCenter.EmitBattleStarted(fixture.BattleContext);

			BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
			fixture.CompletePlacement(
				orchestrator,
				playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
				enemyTurnBars: new[] { 0f });

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			BattleFeatEventReporter.Emit(
				fixture.EnemyUnits[0],
				new DealDamageRequirement.Event { Amount = 50 });

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatBoardService.FindNodeProgress(fixture.EnemySources[0], damageNode);
			bool wasCompleted = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(wasCompleted, Is.False);

			orchestrator.Dispose();
		}
	}
}
