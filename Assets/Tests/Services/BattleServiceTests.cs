using NUnit.Framework;

namespace Tests.Services
{
    public sealed class BattleServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void ResolveBattle_WithActiveContext_EmitsBattleResolvedAndClearsActiveContext()
        {
            using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
            BattleService battleService = ServiceLocator.Instance.BattleService;

            EventCenter.EmitBattleStarted(fixture.BattleContext);

            BattleSide? resolvedWinner = null;
            EventCenter.BattleResolved += OnBattleResolved;

            try
            {
                battleService.ResolveBattle(fixture.BattleContext, BattleSide.Player);

                Assert.That(resolvedWinner, Is.EqualTo(BattleSide.Player));
                Assert.That(battleService.ActiveBattleContext, Is.Null);
            }
            finally
            {
                EventCenter.BattleResolved -= OnBattleResolved;
            }

            void OnBattleResolved(BattleContext ctx, BattleSide winner) => resolvedWinner = winner;
        }
    }
}
