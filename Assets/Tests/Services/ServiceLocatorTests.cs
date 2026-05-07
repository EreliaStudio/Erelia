using NUnit.Framework;

namespace Tests.Services
{
    public sealed class ServiceLocatorTests
    {
        [Test]
        public void Create_WithGameContext_InstantiatesAllServices()
        {
            using ServiceLocatorTestScope scope = new ServiceLocatorTestScope();

            Assert.That(ServiceLocator.Instance, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.BattleService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.BattleActionCompositionService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.PlayerService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.FeatBoardService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.TamingService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.EncounterService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.WorldService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.SaveService, Is.Not.Null);
            Assert.That(ServiceLocator.Instance.IOFileService, Is.Not.Null);
        }
    }
}
