using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
    public sealed class WorldServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void TryBuildBattleBoard_WithoutVoxelRegistry_ReturnsFalse()
        {
            WorldService worldService = ServiceLocator.Instance.WorldService;
            BoardConfiguration config = new BoardConfiguration();

            bool result = worldService.TryBuildBattleBoard(config, Vector3.zero, out BoardData boardData);

            Assert.That(result, Is.False);
            Assert.That(boardData, Is.Null);
        }
    }
}
