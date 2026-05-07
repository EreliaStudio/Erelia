using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Services
{
    public sealed class FeatBoardServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        // -------------------------------------------------------------------------
        // Event accumulation
        // -------------------------------------------------------------------------

        [Test]
        public void BattleFeatEvent_FromPlayer_AccumulatesForResolution()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            SetupFeatBoardWithDamageNode(playerSource, requiredDamage: 10, nodeId: "deal_dmg");

            int progressUpdatedCount = 0;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedCount++;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedCount, Is.EqualTo(1));
        }

        [Test]
        public void BattleFeatEvent_FromEnemy_NotAccumulatedForResolution()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            SetupFeatBoardWithDamageNode(playerSource, requiredDamage: 10, nodeId: "deal_dmg");

            int progressUpdatedCount = 0;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedCount++;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.EnemyUnits[0], new DamageEvent { Amount = 10, Caster = fixture.EnemyUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedCount, Is.EqualTo(0));
        }

        [Test]
        public void MultipleEventsInBattle_AllApplyAtResolution()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            SetupFeatBoardWithDamageNode(playerSource, requiredDamage: 30, nodeId: "deal_dmg");

            int progressUpdatedCount = 0;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedCount++;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedCount, Is.EqualTo(1));
        }

        // -------------------------------------------------------------------------
        // Battle resolution
        // -------------------------------------------------------------------------

        [Test]
        public void BattleResolved_PlayerWin_NodeCompletes_FeatProgressUpdatedFired()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            FeatNode targetNode = SetupFeatBoardWithDamageNode(playerSource, requiredDamage: 10, nodeId: "deal_dmg");

            CreatureUnit receivedUnit = null;
            EventCenter.FeatProgressUpdated += (unit, _) => receivedUnit = unit;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(receivedUnit, Is.SameAs(playerSource));
            Assert.That(FeatBoardService.GetCompletionCount(playerSource, targetNode), Is.GreaterThan(0));
        }

        [Test]
        public void BattleResolved_NodeNotMet_FeatProgressUpdatedNotFired()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            SetupFeatBoardWithDamageNode(playerSource, requiredDamage: 100, nodeId: "deal_dmg");

            bool progressUpdatedFired = false;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedFired, Is.False);
        }

        [Test]
        public void BattleResolved_WrongContext_ProgressNotApplied()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            using BattlePhaseTestFixture otherFixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            SetupFeatBoardWithDamageNode(playerSource, requiredDamage: 10, nodeId: "deal_dmg");

            bool progressUpdatedFired = false;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 10, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(otherFixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedFired, Is.False);
        }

        // -------------------------------------------------------------------------
        // Scope resets
        // -------------------------------------------------------------------------

        [Test]
        public void BattleAbilityResolved_ActionScopeProgress_IsReset()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            FeatNode targetNode = SetupFeatBoardWithDamageNode(
                playerSource,
                requiredDamage: 10,
                nodeId: "deal_dmg",
                scope: FeatRequirement.Scope.Action);

            bool progressUpdatedFired = false;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 5, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleAbilityResolved(fixture.BattleContext, fixture.PlayerUnits[0]);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 5, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedFired, Is.False);
            Assert.That(FeatBoardService.GetCompletionCount(playerSource, targetNode), Is.EqualTo(0));
        }

        [Test]
        public void BattleTurnEnded_TurnScopeProgress_IsReset()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            CreatureUnit playerSource = fixture.PlayerSources[0];
            FeatNode targetNode = SetupFeatBoardWithDamageNode(
                playerSource,
                requiredDamage: 10,
                nodeId: "deal_dmg",
                scope: FeatRequirement.Scope.Turn);

            bool progressUpdatedFired = false;
            EventCenter.FeatProgressUpdated += (_, _) => progressUpdatedFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 5, TurnIndex = 0, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleTurnEnded(fixture.BattleContext, fixture.PlayerUnits[0]);
            EventCenter.EmitBattleEventOccurred(fixture.PlayerUnits[0], new DamageEvent { Amount = 5, TurnIndex = 1, Caster = fixture.PlayerUnits[0] });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(progressUpdatedFired, Is.False);
            Assert.That(FeatBoardService.GetCompletionCount(playerSource, targetNode), Is.EqualTo(0));
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private static FeatNode SetupFeatBoardWithDamageNode(
            CreatureUnit unit,
            int requiredDamage,
            string nodeId,
            FeatRequirement.Scope scope = FeatRequirement.Scope.Fight)
        {
            FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
            FeatNode targetNode = new FeatNode
            {
                Id = nodeId,
                DisplayName = nodeId,
                Requirements = new List<FeatRequirement>
                {
                    new DealDamageRequirement { RequiredAmount = requiredDamage, RequirementScope = scope }
                },
                NeighbourNodeIds = new List<string> { rootNode.Id }
            };
            rootNode.NeighbourNodeIds = new List<string> { targetNode.Id };

            unit.Species.FeatBoard = new FeatBoard
            {
                Nodes = new List<FeatNode> { rootNode, targetNode },
                RootNodeId = rootNode.Id
            };
            FeatBoardService.InitializeCreatureUnit(unit);
            return targetNode;
        }
    }
}
