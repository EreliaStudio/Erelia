using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Services
{
    public sealed class TamingServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        // -------------------------------------------------------------------------
        // Wild unit tracking
        // -------------------------------------------------------------------------

        [Test]
        public void BattleStarted_WildUnitWithProfile_IsTrackedForTaming()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 5 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            WildBattleUnit wildUnit = GetWildUnit(fixture, 0);

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 5 });

            Assert.That(wildUnit.IsTamed, Is.True);
        }

        [Test]
        public void FeatEvent_FromEnemy_DoesNotAdvanceTaming()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 5 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            WildBattleUnit wildUnit = GetWildUnit(fixture, 0);

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleFeatEventOccurred(fixture.EnemyUnits[0], new DealDamageRequirement.Event { Amount = 5 });

            Assert.That(wildUnit.IsTamed, Is.False);
        }

        // -------------------------------------------------------------------------
        // Impression events
        // -------------------------------------------------------------------------

        [Test]
        public void WildUnit_WhenImpressed_CreatureImpressedFired()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 5 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            WildBattleUnit wildUnit = GetWildUnit(fixture, 0);

            BattleContext capturedContext = null;
            WildBattleUnit capturedUnit = null;
            EventCenter.CreatureImpressed += OnCreatureImpressed;

            try
            {
                EventCenter.EmitBattleStarted(fixture.BattleContext);
                EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 5 });

                Assert.That(capturedContext, Is.Not.Null);
                Assert.That(capturedUnit, Is.SameAs(wildUnit));
            }
            finally
            {
                EventCenter.CreatureImpressed -= OnCreatureImpressed;
            }

            void OnCreatureImpressed(BattleContext ctx, WildBattleUnit unit)
            {
                capturedContext = ctx;
                capturedUnit = unit;
            }
        }

        [Test]
        public void WildUnit_WhenImpressed_BattleUnitRemovalRequested()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 5 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            WildBattleUnit wildUnit = GetWildUnit(fixture, 0);

            BattleUnit capturedRemoval = null;
            EventCenter.BattleUnitRemovalRequested += OnRemoval;

            try
            {
                EventCenter.EmitBattleStarted(fixture.BattleContext);
                EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 5 });

                Assert.That(capturedRemoval, Is.SameAs(wildUnit));
            }
            finally
            {
                EventCenter.BattleUnitRemovalRequested -= OnRemoval;
            }

            void OnRemoval(BattleContext ctx, BattleUnit unit) => capturedRemoval = unit;
        }

        [Test]
        public void WildUnit_NotYetImpressed_NoRemovalRequested()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 10 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            bool removalFired = false;
            EventCenter.BattleUnitRemovalRequested += (_, __) => removalFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 5 });

            Assert.That(removalFired, Is.False);
        }

        // -------------------------------------------------------------------------
        // Battle resolution
        // -------------------------------------------------------------------------

        [Test]
        public void BattleResolved_PlayerWin_ImpressedRecruitsReportedViaTamingResolved()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 5 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            IReadOnlyList<CreatureUnit> receivedRecruits = null;
            EventCenter.TamingResolved += OnTamingResolved;

            try
            {
                EventCenter.EmitBattleStarted(fixture.BattleContext);
                EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 5 });
                EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

                Assert.That(receivedRecruits, Is.Not.Null);
                Assert.That(receivedRecruits.Count, Is.EqualTo(1));
            }
            finally
            {
                EventCenter.TamingResolved -= OnTamingResolved;
            }

            void OnTamingResolved(BattleContext ctx, IReadOnlyList<CreatureUnit> recruits) => receivedRecruits = recruits;
        }

        [Test]
        public void BattleResolved_EnemyWin_TamingResolvedNotFired()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 5 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            bool tamingResolvedFired = false;
            EventCenter.TamingResolved += (_, __) => tamingResolvedFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 5 });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Enemy);

            Assert.That(tamingResolvedFired, Is.False);
        }

        [Test]
        public void BattleResolved_PlayerWin_NoImpressedUnits_TamingResolvedNotFired()
        {
            TamingProfile profile = BuildProfile(new DealDamageRequirement { RequiredAmount = 99 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 1,
                enemyTamingProfiles: new[] { profile });

            bool tamingResolvedFired = false;
            EventCenter.TamingResolved += (_, __) => tamingResolvedFired = true;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(tamingResolvedFired, Is.False);
        }

        [Test]
        public void MultipleWildUnits_OneImpressed_OnlyThatOneReportedAsRecruit()
        {
            TamingProfile easyProfile = BuildProfile(new DealDamageRequirement { RequiredAmount = 1 });
            TamingProfile hardProfile = BuildProfile(new DealDamageRequirement { RequiredAmount = 999 });
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(
                playerCount: 1, enemyCount: 2,
                enemyTamingProfiles: new[] { easyProfile, hardProfile });

            IReadOnlyList<CreatureUnit> receivedRecruits = null;
            EventCenter.TamingResolved += (_, recruits) => receivedRecruits = recruits;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleFeatEventOccurred(fixture.PlayerUnits[0], new DealDamageRequirement.Event { Amount = 1 });
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(receivedRecruits, Is.Not.Null);
            Assert.That(receivedRecruits.Count, Is.EqualTo(1));
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private static TamingProfile BuildProfile(params FeatRequirement[] requirements)
        {
            TamingProfile profile = new TamingProfile();
            for (int i = 0; i < requirements.Length; i++)
            {
                profile.Conditions.Add(requirements[i]);
            }

            return profile;
        }

        private static WildBattleUnit GetWildUnit(BattlePhaseTestFixture fixture, int index)
        {
            BattleUnit unit = fixture.EnemyUnits[index];
            Assert.That(unit, Is.InstanceOf<WildBattleUnit>(), $"EnemyUnits[{index}] is not a WildBattleUnit — check that enemyTamingProfiles was set.");
            return (WildBattleUnit)unit;
        }
    }
}
