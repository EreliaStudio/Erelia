using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Services
{
    public sealed class EncounterServiceTests
    {
        private ServiceLocatorTestScope serviceLocatorScope;

        [SetUp]
        public void SetUp() => serviceLocatorScope = new ServiceLocatorTestScope();

        [TearDown]
        public void TearDown() => serviceLocatorScope?.Dispose();

        [Test]
        public void RequestBattle_WithValidConfig_EmitsBattleLaunchRequested()
        {
            EncounterService encounterService = ServiceLocator.Instance.EncounterService;
            BoardConfiguration config = new BoardConfiguration();
            Vector3Int requestedReturnCell = new Vector3Int(3, 4, 5);

            bool launchRequested = false;
            Vector3Int? capturedReturnCell = null;
            EventCenter.BattleLaunchRequested += OnLaunchRequested;

            try
            {
                encounterService.RequestBattle(config, Vector3.zero, requestedReturnCell, null, PlacementStyle.HalfBoard, false);

                Assert.That(launchRequested, Is.True);
                Assert.That(capturedReturnCell, Is.EqualTo(requestedReturnCell));
            }
            finally
            {
                EventCenter.BattleLaunchRequested -= OnLaunchRequested;
            }

            void OnLaunchRequested(BoardConfiguration c, Vector3 pos, Vector3Int? returnCell, IReadOnlyList<EncounterUnit> units, PlacementStyle style, bool allowsTaming)
            {
                launchRequested = true;
                capturedReturnCell = returnCell;
            }
        }
    }
}
