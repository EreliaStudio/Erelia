using NUnit.Framework;

namespace Tests.Services
{
    public sealed class BattleActionCompositionServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void BattleResolved_AfterBattleStarted_ClearsPendingContext()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            BattleActionCompositionService service = ServiceLocator.Instance.BattleActionCompositionService;

            EventCenter.EmitBattleStarted(fixture.BattleContext);
            EventCenter.EmitBattleResolved(fixture.BattleContext, BattleSide.Player);

            Assert.That(service.HasPendingContext, Is.False);
        }
    }
}
